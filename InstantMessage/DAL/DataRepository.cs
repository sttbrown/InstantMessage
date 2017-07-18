using InstantMessage.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
           User currentUser = _Data.Users.Find(authenticatedUser);

            if (currentUser == null)
            {
                return null;
            }

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

        public List<Conversation> getAllConversations(User currentUser)
        {
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


        public List<User> getAllContacts(string User)
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


        public Conversation startConversation(User current, User other)
        {
            Conversation newConversation = new Conversation();

            newConversation.Users.Add(other);
            newConversation.Users.Add(current);

            return newConversation;
        }
            
            
    }
}