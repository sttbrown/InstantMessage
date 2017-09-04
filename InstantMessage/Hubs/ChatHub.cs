using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using InstantMessage.DAL;
using Microsoft.AspNet.Identity;
using InstantMessage.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace InstantMessage
{   /// <summary>
/// Provides all server side methods for the messaging functionality.
/// </summary>
    [RequireHttps]
    [Microsoft.AspNet.SignalR.Authorize]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();  

        private DataRepository _Repo = new DataRepository();
        private string AuthenticatedUser;
        private User CurrentUser;
        
        /// <summary>
        /// Helper method to checks the userid of a calling client and retrieves a User object
        /// </summary>
        private void PersistStateHelper()
        {
            AuthenticatedUser = Clients.Caller.UserId;
            CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);

        }

        /// <summary>
        /// Helper Method to add user to all their relevant groups.
        /// </summary>
        private void InitializeGroups()
        {
            if (CurrentUser != null)
            {
                foreach(var con in CurrentUser.Conversations)
                {
                    //this 'toString' allows ConversationID which is 
                    //an incremental integer to be used as a groupName which must
                    //be a string within SignalR.
                    string groupName = con.ConversationID.ToString();
                    Groups.Add(Context.ConnectionId, groupName);
                }
            }
            else
            {
                Debug.WriteLine("current user is null");
            }
        }

        /// <summary>
        /// Gets the user name and makes it equal to the callers userId to persist state. Passes this
        /// redefined userId back to the client (.initialized()).
        /// </summary>
        /// <returns>base class OnConnected method</returns>
        public override Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
               string userName = Context.User.Identity.GetUserName();
                
                Clients.Caller.UserId = userName;
               _connections.Add(userName, Context.ConnectionId); 
                //map connection to userName.

                Clients.Caller.initialized();
                //empty method "initialized" ensures that client is passed userID to persist state

                PersistStateHelper();
                InitializeGroups();
                UpdateLastActive();

                Clients.Caller.updateUser(AuthenticatedUser);


            }
            else
            {
                Console.WriteLine("problem authenticating");
            }

            return base.OnConnected();
        }

        /// <summary>
        /// Helpermethod updates the date and time that the user was last active.
        /// </summary>
        private void UpdateLastActive()
        {
           if (CurrentUser != null)
           {
                _Repo.UpdateLastActive(CurrentUser);
           }

        }

        /// <summary>
        /// Passes the UserName/ID to the calling signalR client
        /// </summary>
        public void GetUser()
        {
            PersistStateHelper();
            Clients.Caller.updateUser(AuthenticatedUser);
        }

        /// <summary>
        /// Handles client request to access a conversation
        /// </summary>
        /// <param name="conId"></param>
        public void OpenConversation(int conId)
        {
            PersistStateHelper();
            Conversation con = _Repo.getConversation(conId);
            Boolean isAuthorized = false;

            if (con != null)
            {
                isAuthorized = _Repo.CheckAuthorization(CurrentUser, con);

                if (isAuthorized == true)
                {
                    List<Message> messages = _Repo.getMessages(con);

                    //ensures that the client message display is focussed on correct conversation.
                    Clients.Caller.setOnScreenConversation(con.ConversationID);

                    //Batch loads all messages one at a time
                    foreach (Message m in messages)
                    {
                        Clients.Caller.loadMessage(m.User.UserID, m, con.ConversationID);
                    }

                    //passes Information about the participating users to the client
                    foreach(User user in con.Users)
                    {
                        Clients.Caller.addConversationUser(user, con.ConversationID);
                    }

                    //informs client that the conversation has loaded, and can be displayed
                    Clients.Caller.finishedLoadingConversation(con.ConversationID);
                }
                else
                {
                    Debug.WriteLine("user not authorised to access this conversation");
                }
            }
        }

        /// <summary>
        /// Passes all contacts to the client
        /// </summary>
        public void GetContacts()
        {
            PersistStateHelper();

            //this passes all contacts.
            List<User> users = _Repo.GetAllContacts(AuthenticatedUser);

            foreach (User u in users)
            {
                Clients.Caller.PassContact(u);
            }
            //informs client that the contacts are loaded, and can be displayed
            Clients.Caller.ShowContacts();
           
        }

        /// <summary>
        /// Starts a new conversation and sends the first message
        /// </summary>
        /// <param name="message">represents the clients message</param>
        /// <param name="contacts">list containing recipients for the message</param>
        /// <param name="conversationName">representing conversation name, could be null if not added</param>
        public void SendFirstMessage(string message, List<string> contacts, string conversationName)
        {
            PersistStateHelper();

            if (message != null)
            {
                //Adds the caller to the conversation so they too are included in conversation
                contacts.Add(AuthenticatedUser);
                Conversation con = StartConversation(contacts, conversationName);

                Clients.Caller.setOnScreenConversation(con.ConversationID);

                Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);

                PassNewConversationToClients(con, conversationMessage);
            }
            else
            {
                Debug.WriteLine("message is null");
            }
        }
        /// <summary>
        /// helper method to start new conversation
        /// </summary>
        /// <param name="con">representing the conversation</param>
        /// <param name="message">representing the message</param>
        private void PassNewConversationToClients(Conversation con, Message message)
        {
            foreach(User u in con.Users)
            { 
                Clients.User(u.UserID).receiveNewConversation(con);
                Clients.User(u.UserID).messageHandler(message, u.UserID, con.ConversationID);
            }

        }

        /// <summary>
        /// Method to allow the client to join a conversation using 
        /// </summary>
        /// <param name="conID">representing the conversation id</param>
        public void JoinGroupRemotely(string conID)
        {
            PersistStateHelper();
            string groupName = conID;
            Groups.Add(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Helper method to create a new group usin 
        /// </summary>
        /// <param name="con">representing the conversation object</param>
        private void CreateNewGroup(Conversation con)
        {
            if (CurrentUser != null)
            {
                string groupName = con.ConversationID.ToString();
                Groups.Add(Context.ConnectionId, groupName);
            }

        }

        /// <summary>
        /// Send a message as part of an existing conversation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="conversationID"></param>
        public void Send(string message, int conversationID)
        {
            PersistStateHelper();

            //Should the message be sanitised any further?

            if (message != null)
            {
                Conversation con = _Repo.getConversation(conversationID);
                Boolean isAuthorized = _Repo.CheckAuthorization(CurrentUser, con);

                if (isAuthorized == true)
                {
                    Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);
                    UpdateClient(conversationMessage, con);
                }
                else
                {
                    Debug.WriteLine("User not authorised");                }
                }
            else
            {
                Debug.WriteLine("no message present");
            }
        }
        
        /// <summary>
        /// Helper method to update the client conversations and messages
        /// </summary>
        /// <param name="conversationMessage">Message object representing the current message object</param>
        /// <param name="con">Conversation object representing the current Conversation</param>
        private void UpdateClient(Message conversationMessage, Conversation con)
        {
            string groupName = con.ConversationID.ToString();
            
            //ensures the calling client has the conversation set live on screen (extra check potentially superflous)
            Clients.Caller.setOnScreenConversation(con.ConversationID);

            //updates conversation panel and conversation object
            Clients.Group(groupName).updateExistingConversation(con);
            
            //sends message to client
            Clients.Group(groupName).messageHandler(conversationMessage, conversationMessage.User.UserID, con.ConversationID);

        }

        /// <summary>
        /// Helper method to initiate new conversation and return id to the calling client.
        /// </summary>
        /// <param name="contacts">represents recipient users</param>
        /// <param name="conversationName">represents conversation name. Could be null if no name given.</param>
        /// <returns>Conversation Object</returns>
        private Conversation StartConversation(List<string> contacts, string conversationName)
        {
            List<User> users = _Repo.retrieveUsers(contacts);

            Conversation con = _Repo.startConversation(users, conversationName);
            
            Clients.Caller.ReturnConversationDetails(con.ConversationID);
            
            return con;
        }

        /// <summary>
        /// Passes all participating conversations to the calling client 
        /// </summary>
        public void GetAllConversations()
        {
            PersistStateHelper();
            List<Conversation> conversations = _Repo.GetAllConversations(CurrentUser);            
          
            if (conversations != null)
            {
                foreach (Conversation c in conversations)
                {
                    Clients.Caller.AddExistingConversation(c);
                }

                Clients.Caller.AllConversationsAdded();
            }
        }


        /// <summary>
        /// Search function not implemented
        /// </summary>
        /// <param name="searchFor"></param>
        public void Search(string searchFor)
        {

        }

    /// <summary>
    /// Handles client reconnection
    /// </summary>
    /// <returns>base method OnReconnected</returns>
    public override Task OnReconnected()
    {
            string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }
   
            PersistStateHelper();

            if (CurrentUser != null)
            {
                UpdateLastActive();
            }    
            return base.OnReconnected();
    }

    /// <summary>
    /// Over-ridden method to handle disconnection process.
    /// </summary>
    /// <param name="stopCalled"></param>
    /// <returns></returns>
    public override Task OnDisconnected(bool stopCalled)
    {
            string disconnectedUser = Context.User.Identity.Name;

            //updates the exact time the user has logged out.
            _Repo.UpdateLastActiveDisconnected(disconnectedUser);

            //Uncomment this line to remove the connectionID from the in-memory storage
           // _connections.Remove(disconnectedUser, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
    }

    }
}