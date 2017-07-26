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
            CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);
        }


        public override Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {

                Clients.Caller.UserId = Context.User.Identity.GetUserName();
                Clients.Caller.initialized();

            }
            else
            {
                Console.WriteLine("problem authenticating");
                //do something eg return to log-in page?
            }

            //if (Context.User.Identity.IsAuthenticated)
            //{
            //    AuthenticatedUser = Context.User.Identity.GetUserName();
            //    CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);

            //}
            //else
            //{
            //    Console.WriteLine("problem authenticating");
            //    //do something eg return to log-in page?
            //}

            ////




            // EXCEPTION AT TIMES
            //try
            //{
            //    AuthenticatedUser = HttpContext.Current.GetOwinContext()
            //    .Authentication.User.Identity.GetUserName();


            //    var user = Context.User;
            //    string name = user.Identity.Name;

            //    // AuthenticatedUser = "s.t.t.brown@hotmail.co.uk";

            //    CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);


            //    Debug.WriteLine("user.Identity.Name =  " + name);
            //    Debug.WriteLine("AuthenticatedUser =  " + AuthenticatedUser);


            //}
            //catch
            //{
            //    Debug.WriteLine("authentication exception");
            //}

            return base.OnConnected();
        }

        public void Contacts()
        {
            //List<User> contacts = _Repo.getAllContacts(CurrentUser.UserID);

            string john = "John";

            Clients.All.display(john);


        }


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
            Debug.WriteLine(message + "FIRST MESSAGE ");

            PersistStateHelper();

            if (message != null)
            {
                contacts.Add(AuthenticatedUser);
                Conversation con = StartConversation(contacts, conversationName);

                Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);
                UpdateMessageOnClient(conversationMessage);

            }
        }

        public void Send(string message, int conversationID)
        {
            PersistStateHelper();

            if (message != null)
            {
                //get Conversation, then call generateMessage
                Conversation con = _Repo.getConversation(conversationID);
                Debug.WriteLine(message + "OTHER MESSAGE " + con.ConversationID);

                Message conversationMessage = _Repo.GenerateMessage(message, CurrentUser, con);
                UpdateMessageOnClient(conversationMessage);
            }
        }


        private void UpdateMessageOnClient(Message conversationMessage)
        {
            //    //updates all connected clients 
            Clients.All.addNewMessageToPage(conversationMessage.User.UserID, conversationMessage.Content);


        }


        private Conversation StartConversation(List<string> contacts, string conversationName)
        {
            List<User> users = _Repo.retrieveUsers(contacts);

            Conversation con = _Repo.startConversation(users, conversationName);

           // con.ConversationID = "Example Conversation";
            
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