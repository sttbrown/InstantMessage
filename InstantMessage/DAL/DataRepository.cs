using InstantMessage.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace InstantMessage.DAL
{
    public class DataRepository
    {
        private InstantMessageContext _Data;

        public DataRepository()
        {
             _Data = new InstantMessageContext();
           
        }

        public void saveChanges()
        {
            _Data.SaveChangesAsync(); 
        }

        public User getCurrentUser(string authenticatedUser)
        {
            // User currentUser = _Data.Users.Find(authenticatedUser);
            User currentUser = _Data.Users.Where(u => u.UserID == authenticatedUser)
                .FirstOrDefault();

            return currentUser;
        }


        public void createNewUser(string authenticatedUser)
        {
            try
            {
                User currentUser = new User(authenticatedUser);
                _Data.Users.Add(currentUser);
                _Data.SaveChanges();

            }
            catch (Exception e)
            {
               Console.WriteLine("problem"); //do something else?
            }

            return;
        }

        public List<Conversation> GetAllConversations(User currentUser)
        {
            //REFACTOR THIS!!!
            //not efficient 
            List<Conversation> conver = new List<Conversation>();

            List<Conversation> userCon = new List<Conversation>();

            try
            {
                conver = _Data.Conversations.ToList();

                foreach (Conversation c in conver)
                {
                    if (c.Users.Contains(currentUser))
                    {
                        userCon.Add(c);
                    }
                }


            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("no conversations available");
                return null;
            }           

            return userCon;
        }

        public void addMessageToConversation(Message m, string conversationID )
        {
            var conversation = _Data.Conversations.Find(m.Conversation);
            conversation.Messages.Add(m);

            try
            {
                _Data.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                Debug.WriteLine("exception caught dataRepo.addMessageToCon ");
            }
        }

        public void AddMessageToUser(Message m)
        {
            
        }


        public Message GenerateMessage(string message, User currentUser, Conversation con)
        {
            Message m = new Message();

                m.Content = message;
                m.User = currentUser;
                m.Conversation = con;

                con.Messages.Add(m);

                _Data.Messages.Add(m);

                _Data.SaveChanges();

           

            return m;
        }

        //this is ALL other users, not yet contacts
        public List<User> GetAllContacts(string User)
        {
            List<User> contacts = new List<User>();
            List<User> all = _Data.Users.ToList();

            foreach (User u in all)
            {
                if (u.UserID != User)
                {
                    contacts.Add(u);
                }
            }
            return contacts;
        }

       public User getContact(string userID)
       {
            return _Data.Users.Find(userID);
       }


        public Conversation getConversation(int conversationID)
        {
            return _Data.Conversations.Find(conversationID);
        }



        public List<User> retrieveUsers(List<string> contacts)
        {
            List<User> users = new List<User>();

            foreach(string s in contacts)
            {
                users.Add(_Data.Users.Find(s));
            }
            return users;
        }


        public Conversation startConversation(List<User> users, string conversationName)
        {
            Conversation newConversation = new Conversation();

            if (conversationName != null)
            {
                newConversation.Name = conversationName;
                Debug.WriteLine("data repository sets con name = " + conversationName);
            }
            else
            {
                string name = "";
                try
                {
                    
                    foreach (User u in users)
                    {
                        //once user NAMES have been sorted out this can be changed
                        name += u.UserID + ", ";
                    }
                   
                }
                catch(NullReferenceException)
                {
                    Debug.WriteLine("do something");
                }

                newConversation.Name = name;
            }
           

            foreach (User u in users)
            {
                newConversation.Users.Add(u);
            }

            _Data.Conversations.Add(newConversation);

            //Save conversation??
            try
            {
                _Data.SaveChanges();
            }
            catch(System.Data.Entity.Validation.DbEntityValidationException e)
            {
                Debug.WriteLine("exception caught datarepo.startConversation");
            }
            

            return newConversation;
        }


        public Boolean CheckAuthorization(User current, Conversation con)
        {
            Boolean isAuthorized = false;

            //var user = _Data.Users
            //        .Include(u => u.Conversations)
            //        .SingleOrDefault(u => u.UserID == current.UserID);

            //get conversation, check users in conversation, 

            if (con.Users.Contains(current))
            {
                isAuthorized = true;
            }
            else
            {
                isAuthorized = false;
            }

            return isAuthorized;
        }


        public List<Message> getMessages(Conversation con)
        {

            // students = students.OrderBy(s => s.EnrollmentDate);
           // Conversation conInOrder = con.Messages.OrderBy(m => m.Sent);
           // List<Message> messages = conInOrder.Messages.ToList();
            List<Message> messages = con.Messages.ToList();

            return messages;
        }

            
            
    }
}