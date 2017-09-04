using InstantMessage.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace InstantMessage.DAL
{   
    /// <summary>
    /// Class interfaces between the server side hub methods and persistent storage.
    /// Also responsible for C# object generation.
    /// </summary>
    public class DataRepository
    {
        private InstantMessageContext _Context;

        public DataRepository()
        {
             _Context = new InstantMessageContext();             
        }

        /// <summary>
        /// Save Changes to the database
        /// </summary>
        public void saveChanges()
        {
            _Context.SaveChangesAsync(); 
        }

        /// <summary>
        /// Method to return the current user
        /// </summary>
        /// <param name="authenticatedUser">string representing the userID</param>
        /// <returns>User object representing the current user</returns>
        public User getCurrentUser(string authenticatedUser)
        {           
            try
            {
                User currentUser = _Context.Users.Where(u => u.UserID == authenticatedUser)
                               .FirstOrDefault();
                return currentUser;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception= " + e);
                return null;
            }             
        }

        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="authenticatedUser">representing the authenticated email address</param>
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
                Console.WriteLine("problem creating user. Exception=  "+ e);
            } 
        }

        /// <summary>
        /// Method to update the user last active property. Further iteration could improve accuracy of this. 
        /// </summary>
        /// <param name="current">Representing the current user</param>
        public void UpdateLastActive(User current)
        {
            current.LastActive = "Today";
            _Context.SaveChanges();

        }

        /// <summary>
        /// Updates the users lastactive attribute upon
        /// </summary>
        /// <param name="disconnectedUser">representing the disconnecting user</param>
        public void UpdateLastActiveDisconnected(string disconnectedUser)
        {
            User leavingUser = getCurrentUser(disconnectedUser);

            if (leavingUser != null)
            {
                leavingUser.LastActive = DateTime.Now.ToString("g");
                _Context.SaveChanges();
            }
        }

        /// <summary>
        /// Returns users conversations
        /// </summary>
        /// <param name="currentUser">represents the user object</param>
        /// <returns>List of Conversations</returns>
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
        }

        /// <summary>
        /// Creates a New Message
        /// </summary>
        /// <param name="message">represents the message content</param>
        /// <param name="currentUser">represents the current users</param>
        /// <param name="con">represents the current conversation</param>
        /// <returns>Message object</returns>
        public Message GenerateMessage(string message, User currentUser, Conversation con)
        {
            Message m = new Message();

            m.Content = message;
            m.User = currentUser;
            m.Conversation = con;

            con.LastEdited = DateTime.Now.ToString("s");

            con.LastMessage = ""+currentUser.UserID+": "+ m.Content;
            con.Messages.Add(m);

            _Context.Messages.Add(m);
            _Context.SaveChanges();
             return m;
        }

        /// <summary>
        /// Returns all other users. Future version would include logic for determining which users were connected
        /// and which were not.
        /// </summary>
        /// <param name="User">represents the</param>
        /// <returns>List of all contacts</returns>
        public List<User> GetAllContacts(string User)
        {
            List<User> contacts = new List<User>();
            List<User> all = _Context.Users.ToList();

            foreach (User u in all)
            {
                //do not want to add calling users details as a contact
                if (u.UserID != User)
                {
                    contacts.Add(u);
                }
            }
            return contacts;
        }
        
        /// <summary>
        /// Retrieves a contact based upon a userID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>User object representing the requested user</returns>
       public User getContact(string userID)
       {
            return _Context.Users.Find(userID);
       }

        /// <summary>
        /// Search for a return a conversation
        /// </summary>
        /// <param name="conversationID">integer identifying a conversation id</param>
        /// <returns></returns>
        public Conversation getConversation(int conversationID)
        {
            return _Context.Conversations.Find(conversationID);
        }


        /// <summary>
        /// finds users corresponding to a list of string representating contacts
        /// </summary>
        /// <param name="contacts">Representing the userid's of contacts</param>
        /// <returns></returns>
        public List<User> retrieveUsers(List<string> contacts)
        {
            List<User> users = new List<User>();

            foreach(string s in contacts)
            {
                users.Add(_Context.Users.Find(s));
            }
            return users;
        }

        /// <summary>
        /// Starts a new convrsation.
        /// </summary>
        /// <param name="users">List of participating users</param>
        /// <param name="conversationName">name of conversation</param>
        /// <returns></returns>
        public Conversation startConversation(List<User> users, string conversationName)
        {
            Conversation newConversation = new Conversation();

            if (conversationName != null)
            {
                newConversation.Name = conversationName;
            }
            else
            {
                //if no name has been given define name with reference to all participants
                string name = "";
                try
                {
                    foreach (User u in users)
                    {
                        name += u.UserID + ", ";
                    }
                   
                }
                catch(NullReferenceException)
                {
                    Debug.WriteLine("No contacts in the conversation");
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
            catch(System.Data.Entity.Validation.DbEntityValidationException)
            {
                Debug.WriteLine("exception caught datarepo.startConversation");
            }
            return newConversation;
        }

        /// <summary>
        /// Checks if the user is authorised to access conversation
        /// </summary>
        /// <param name="current">the current users</param>
        /// <param name="con">the current conversation</param>
        /// <returns>boolean representing whether user is authorised</returns>
        public Boolean CheckAuthorization(User current, Conversation con)
        {
            Boolean isAuthorized = false; 

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

        /// <summary>
        /// Returns the messages relevant to a specific conversation
        /// </summary>
        /// <param name="con">the relevant conversation</param>
        /// <returns>List containing all messages of requested conversation</returns>
        public List<Message> getMessages(Conversation con)
        {      
            List<Message> messages = con.Messages.ToList();

            return messages;
        }

        /// <summary>
        /// Search method. Not implemented in this iteration.
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="Current"></param>
        /// <returns></returns>
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