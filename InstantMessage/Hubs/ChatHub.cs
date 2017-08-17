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

namespace InstantMessage
{
    [RequireHttps]
    [Microsoft.AspNet.SignalR.Authorize]
    public class ChatHub : Hub
    {
        private DataRepository _Repo = new DataRepository();
        private string AuthenticatedUser;
        private User CurrentUser;

        
        public ChatHub()
        {

            

        }

        private void PersistStateHelper()
        {
            //Could the client potentially tamper with this Caller.UserId? 
            //can an additional 'session' check be added here?
            AuthenticatedUser = Clients.Caller.UserId;
            CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);

        }


        private void InitializeGroups()
        {
            if (CurrentUser != null)
            {
                foreach(var con in CurrentUser.Conversations)
                {
                    //this 'toString' is a hack that allows ConversationID which is 
                    //an incremental int to be used as a groupName which must
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

        
        public override Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                //I'm not sure if this is secure?
                Clients.Caller.UserId = Context.User.Identity.GetUserName();
                Clients.Caller.initialized();
                //empty method "initialized" ensures that client is passed userID to persist state

                PersistStateHelper();
                InitializeGroups();
                Clients.Caller.updateUser(AuthenticatedUser);


            }
            else
            {
                Console.WriteLine("problem authenticating");
                //do something eg return to log-in page?
            }

            return base.OnConnected();
        }


        public void GetUser()
        {
            PersistStateHelper();
            Clients.Caller.updateUser(AuthenticatedUser);

          //  return AuthenticatedUser;
        }

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
                    Debug.WriteLine("isAuthorised = true");
                    //given authorisation return conversation history
                    //to client.

                    List<Message> messages = _Repo.getMessages(con);

                    //implement some sort of batch loading here?

                    //set OnScreen client variable here:::
                    Clients.Caller.setOnScreenConversation(con.ConversationID);


                    foreach (Message m in messages)
                    {

                        Clients.Caller.loadMessage(m.User.UserID, m, con.ConversationID);
                    }

                    //add condition: if loadMessage Invoked Correctly 
                    //MAKE SURE ASYNCHRONOUS

                    Clients.Caller.finishedLoadingConversation(con.ConversationID);
                }
                else
                {
                    Debug.WriteLine("user not authorised to access this conversation");
                    //add client side message
                }
            }
        }


        public void GetContacts()
        {
            PersistStateHelper();

            //this passes all contacts.
            List<User> users = _Repo.GetAllContacts(AuthenticatedUser);
            List<String> contacts = new List<String>();

            foreach (User u in users)
            {
                //this will need to be User object once names included (JSON)
                Clients.Caller.PassContact(u);
            }
            Clients.Caller.ShowContacts();
           
        }


        public void SendFirstMessage(string message, List<string> contacts, string conversationName)
        {
            //Is user authorised to contact these Users??

            Debug.WriteLine(conversationName  + "CONVERSATION NAME ,  FIRST MESSAGE ");

            PersistStateHelper();

            if (message != null)
            {
                contacts.Add(AuthenticatedUser);
                Conversation con = StartConversation(contacts, conversationName);

                //creates new signalR group so that participants are
                //notified
                // CreateNewGroup(con);

                //Make other users in new conversation join group.
                //InformUsersNewConversation();
                //need to load conversation onto clients screen

                Clients.Caller.setOnScreenConversation(con.ConversationID);

                Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);

                PassNewConversationToClients(con, conversationMessage);

              //  UpdateMessageOnClient(conversationMessage, con);


                

            }
            else
            {
                Debug.WriteLine("message is null");
            }
        }

        private void PassNewConversationToClients(Conversation con, Message message)
        {
            foreach(User u in con.Users)
            { 
                Clients.User(u.UserID).receiveNewConversation(con);
               
               // Clients.User(u.UserID).newMessageNotification(con.ConversationID);
                Clients.User(u.UserID).messageHandler(message, u.UserID, con.ConversationID);
            }

        }

        public void JoinGroupRemotely(string conID)
        {
            //concerns about race conditions here?
            PersistStateHelper();
            string groupName = conID;
            Groups.Add(Context.ConnectionId, groupName);
        }

        private void CreateNewGroup(Conversation con)
        {
            if (CurrentUser != null)
            {
                string groupName = con.ConversationID.ToString();
                Groups.Add(Context.ConnectionId, groupName);
            }

        }

        public void Send(string message, int conversationID)
        {
            PersistStateHelper();

            //carry out sanitation of all user input.

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
                    Debug.WriteLine("User not authorised");
                    //add Client Method to communicate this
                }
            }
            else
            {
                Debug.WriteLine("no message present"); //this is an extra check, done client side aswell
            }
        }

        private void UpdateClient(Message conversationMessage, Conversation con)
        {
            string groupName = con.ConversationID.ToString();

            Clients.Caller.setOnScreenConversation(con.ConversationID);

            //updates conversation panel and conversation object
            Clients.Group(groupName).updateExistingConversation(con);
            
            //sends message to client
            Clients.Group(groupName).messageHandler(conversationMessage, conversationMessage.User.UserID, con.ConversationID);

        }


        //this is back up for now
        //private void UpdateMessageOnClient(Message conversationMessage, Conversation con)
        //{
        //    string groupName = con.ConversationID.ToString();
        //    int conversationId = con.ConversationID;

        //    //Sanitise for display
        //    // string htmlContent = System.Net.WebUtility.HtmlEncode(conversationMessage.Content);

        //    //Also inform all users that they have received new message
        //    Clients.Group(groupName).newMessageNotification(con.ConversationID);
            
        //    //updates all connected clients within that group with message details 
        //    Clients.Group(groupName).transferMessage(conversationMessage, conversationMessage.User.UserID, conversationId);
        //    //Clients.Group(groupName).updateConversations(); //this is now done with transfermessage
        //}


        private Conversation StartConversation(List<string> contacts, string conversationName)
        {
            List<User> users = _Repo.retrieveUsers(contacts);

            Conversation con = _Repo.startConversation(users, conversationName);
            
            Clients.Caller.ReturnConversationDetails(con.ConversationID);
            
            return con;
        }



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



        public void Search(string searchFor)
        {
            PersistStateHelper();

            //Not implemented.


        }


        public override Task OnReconnected()
    {
            PersistStateHelper();
            return base.OnReconnected();
    }

    public override Task OnDisconnected(bool stopCalled)
    {
            // delete the association between the current connection and user name.
            Clients.Caller.UserId = null;

            return base.OnDisconnected(stopCalled);
    }


    }
}