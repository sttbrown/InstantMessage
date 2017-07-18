using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using InstantMessage.DAL;
using Microsoft.AspNet.Identity;
using InstantMessage.Models;
using System.Collections.Generic;

namespace InstantMessage
{
    //authorisation tag here
    public class ChatHub : Hub
    {
        private DataRepository _Repo;
        //private string AuthenticatedUser = System.Web.HttpContext.Current.GetOwinContext()
          //  .Authentication.User.Identity.GetUserId();
        private User CurrentUser;

        public ChatHub()
        {
            _Repo = new DataRepository();
         //   CurrentUser = _Repo.getCurrentUser(AuthenticatedUser);
            
        }


        public void Contacts()
        {
            //List<User> contacts = _Repo.getAllContacts(CurrentUser.UserID);

            string john = "John";

            Clients.All.display(john);


        }


        public void StartConversation(string otherUser)
        {
            User other = _Repo.getContact(otherUser);
            User user = _Repo.getContact(getThisUserId());

            Conversation newCon = new Conversation();
            newCon.Users.Add(other);
            newCon.Users.Add(user);

            //saving should probably not happen until first message sent
            //may not be feasible.
            // _Repo.saveChanges();
            Clients.Caller.ConversationReference(newCon.ConversationID);



        }

        

        private string getThisUserId()
        {
            var user = Context.User;
            string userId = user.Identity.GetUserId();
            return userId;
        }

        public List<string> DisplayContacts()
        {
            var user = Context.User;
           string name = user.Identity.Name;


            List<User> users = _Repo.getAllContacts(name);
            List<String> contacts = new List<String>();

            foreach(User u in users)
            {
                contacts.Add(u.UserID);
            }

            return contacts;

        }

        public void DisplayConversation()
        {

        }

        public void Send(string name, string message, string conversationID)
        {
            var user = Context.User;

            if (user.Identity.IsAuthenticated)
            {
                name = user.Identity.Name;
             
            }
            else
            {
                name = "anonymous";
            }

            //save to conversation/database



            //updates all connected clients 
            Clients.All.addNewMessageToPage(name, message);
        }


        public void GetLatestMessages()
        {
            //Could be used to update all clients as they join?
            //or best to do so from controller?
        }

    }

}