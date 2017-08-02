using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using InstantMessage.DAL;
using Microsoft.AspNet.Identity;
using InstantMessage.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InstantMessage
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static DataRepository _Repo = new DataRepository();
        private static string AuthenticatedUser;
        private User CurrentUser;

        
        public ChatHub()
        {

            

        }

        private void PersistStateHelper()
        {
            AuthenticatedUser = Clients.Caller.UserId;
            CurrentUser = _Repo.getCurrentUser(AuthenticatedUser); //not efficient?
        }


        private void InitializeGroups()
        {
            if (CurrentUser != null)
            {
                foreach(var con in CurrentUser.Conversations)
                {
                    //this is a hack that allows ConversationID which is 
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
                Clients.Caller.UserId = Context.User.Identity.GetUserName();
                Clients.Caller.initialized();
                //empty method "initialized" ensures that client is passed userID to persist state

                PersistStateHelper();
                InitializeGroups();

            }
            else
            {
                Console.WriteLine("problem authenticating");
                //do something eg return to log-in page?
            }

            return base.OnConnected();
        }


        public void OpenConversation(int conId)
        {
            PersistStateHelper();
            Debug.WriteLine("The ConID is " + conId);
            //server side check if user is authorised to view that
            //conversation
           Conversation con= _Repo.getConversation(conId);

            Boolean isAuthorized= false;

            if (con != null)
            {
                isAuthorized = _Repo.CheckAuthorization(CurrentUser, con);

                if (isAuthorized == true)
                {
                    Debug.WriteLine("isAuthorised = true");
                    //given authorisation return conversation history
                    //to client. 
                    List<Message> messages = _Repo.getMessages(con);

                    foreach(Message m in messages)
                    {
                        Clients.Group("" + con.ConversationID).loadMessage(m.User.UserID, m.Content);
                    }



                }
                else
                {
                    Debug.WriteLine("user not authorised to access this conversation");
                    //add client side message
                }
            }
             





        }


        //this needs to be changed so that instead of 
        //return type a client method is called.
        public List<string> DisplayContacts()
        {
            PersistStateHelper();

            List<User> users = _Repo.GetAllContacts(AuthenticatedUser);
            List<String> contacts = new List<String>();

            foreach (User u in users)
            {
                contacts.Add(u.UserID);
            }

            return contacts;

        }


        public void SendFirstMessage(string message, List<string> contacts, string conversationName)
        {
            //Is user authorised to contact these Users??

            Debug.WriteLine(message + "FIRST MESSAGE ");

            PersistStateHelper();

            if (message != null)
            {
                contacts.Add(AuthenticatedUser);
                Conversation con = StartConversation(contacts, conversationName);

                Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);
                UpdateMessageOnClient(conversationMessage, con);

            }
            else
            {
                Debug.WriteLine("message is null");
            }
        }

        public void Send(string message, int conversationID)
        {
            PersistStateHelper();

            if (message != null)
            {
                 Conversation con = _Repo.getConversation(conversationID);

                Boolean isAuthorized = _Repo.CheckAuthorization(CurrentUser, con);

                if (isAuthorized == true)
                {


                    Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);
                    UpdateMessageOnClient(conversationMessage, con);

                }
                else
                {
                    Debug.WriteLine("User not authorised");
                }

            }
            else
            {
                Debug.WriteLine("no message present"); //this is an extra check, done client side aswell
            }
        }


        private void UpdateMessageOnClient(Message conversationMessage, Conversation con)
        {
            string groupName = con.ConversationID.ToString();
           
            //    //updates all connected clients 
            //Clients.All.addNewMessageToPage(conversationMessage.User.UserID, conversationMessage.Content);

            //updates all connected clients within that group. 
            Clients.Group(groupName).addNewMessageToPage(conversationMessage.User.UserID, conversationMessage.Content);


        }


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

            int n = conversations.Count;
          
           foreach (Conversation c in conversations)
           {
                Clients.Caller.AddExistingConversation(c);
           }

        }




   

    //public override Task OnDisconnected()
    //{
    //    // Add your own code here.
    //    // For example: in a chat application, mark the user as offline, 
    //    // delete the association between the current connection id and user name.
    //    return base.OnDisconnected();
    //}

    //public override Task OnReconnected()
    //{
    //    // Add your own code here.
    //    // For example: in a chat application, you might have marked the
    //    // user as offline after a period of inactivity; in that case 
    //    // mark the user as online again.
    //    return base.OnReconnected();
    //}


    }
}