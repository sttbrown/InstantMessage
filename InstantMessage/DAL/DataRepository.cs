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
        private InstantMessageContext _Context;

        public DataRepository()
        {
             _Context = new InstantMessageContext();             
        }

        public void saveChanges()
        {
            _Context.SaveChangesAsync(); 
        }

        public User getCurrentUser(string authenticatedUser)
        {
            // User currentUser = _Data.Users.Find(authenticatedUser);
            //InvalidOperationException: There is already an open DataReader associated with this Command which must be closed first.

                User currentUser = _Context.Users.Where(u => u.UserID == authenticatedUser)
               .FirstOrDefault();

                return currentUser;   
        }


        public void createNewUser(string authenticatedUser)
        {
            try
            {
                User currentUser = new User(authenticatedUser);
                _Context.Users.Add(currentUser);
                _Context.SaveChanges();
            }
            catch (Exception e)
            {
               Console.WriteLine("problem"); //do something else?
            } 
        }

        public void UpdateLastActive(User current)
        {
            current.LastActive = "Today";
            _Context.SaveChanges();

        }

        public void UpdateLastActiveDisconnected(string disconnectedUser)
        {
            User leavingUser = getCurrentUser(disconnectedUser);

            Debug.WriteLine("Update Last ACTIVE METHOD");


            if (leavingUser != null)
            {
                Debug.WriteLine("USER NOT NULL");

                leavingUser.LastActive = DateTime.Now.ToString("g");
                _Context.SaveChanges();
            }
        }


        public List<Conversation> GetAllConversations(User currentUser)
        {
            var results = _Context.Conversations.Where(c => c.Users.Select(u => u.UserID).Contains(currentUser.UserID));

            if (results != null)
            {
                List<Conversation> userCon = results.ToList();
                userCon.Sort((x, y) => x.LastEdited.CompareTo(y.LastEdited));
                userCon.Reverse();
                return userCon;
            }
            else
            {
                return null;
            }

           //is this necessary? could iterate through backwards in chathub instead?

            

            //Is this enough to activate the Last Edited Index?
            //var userCon = results.OrderBy(c => c.LastEdited);
            //List<Conversation> cons = new List<Conversation>();
            //if (userCon != null)
            //{
            //    cons = userCon.ToList();
            //}

            //return cons;      
        }


        public Message GenerateMessage(string message, User currentUser, Conversation con)
        {
            Message m = new Message();

            m.Content = message;
            m.User = currentUser;
            m.Conversation = con;

            con.LastEdited = DateTime.Now.ToString("g");
            //perhaps last message and last message sender should be separated at level of models..
            con.LastMessage = ""+currentUser.UserID+": "+ m.Content;
            con.Messages.Add(m);

            _Context.Messages.Add(m);
            _Context.SaveChanges();

             return m;
        }

        //this is ALL other users, not yet contacts
        public List<User> GetAllContacts(string User)
        {
            List<User> contacts = new List<User>();
            List<User> all = _Context.Users.ToList();

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
            return _Context.Users.Find(userID);
       }


        public Conversation getConversation(int conversationID)
        {
            return _Context.Conversations.Find(conversationID);
        }



        public List<User> retrieveUsers(List<string> contacts)
        {
            List<User> users = new List<User>();

            foreach(string s in contacts)
            {
                users.Add(_Context.Users.Find(s));
            }
            return users;
        }


        public Conversation startConversation(List<User> users, string conversationName)
        {
            Conversation newConversation = new Conversation();

            if (conversationName != null)
            {
                newConversation.Name = conversationName;
            }
            else
            {

                Debug.WriteLine("data repository sets con name = " + conversationName);

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

            newConversation.LastEdited = DateTime.Now.ToString("g");

            _Context.Conversations.Add(newConversation);

            try
            {
                _Context.SaveChanges();
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

           // Conversation conInOrder = con.Messages.OrderBy(m => m.Sent);
           //List<Message> messages = conInOrder.Messages.ToList();
          
            //Appears to be naturally in order. 
            List<Message> messages = con.Messages.ToList();

            return messages;
        }

        public List<Message> Search(string searchFor, User Current)
        {
            //NOT CORRECT::: 

            char[] delimiters = { ' ', ',', '.', ':', '\t' };

            var searchWords = searchFor.Split(delimiters);

            var results = _Context.Messages.Where(r => searchWords.Any(s => r.Content.Contains(s)));

            return results.ToList();
            
        }



            
            
    }
}