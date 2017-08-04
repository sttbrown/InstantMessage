﻿using System;
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

        private void CreateNewGroup(Conversation con)
        {
            if (CurrentUser != null)
            {
                string groupName = con.ConversationID.ToString();
                Groups.Add(Context.ConnectionId, groupName);
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


       
        public void DisplayContacts()
        {
            PersistStateHelper();

            //this passes all contacts.
            List<User> users = _Repo.GetAllContacts(AuthenticatedUser);
            List<String> contacts = new List<String>();

            foreach (User u in users)
            {
                //this will need to be User object once names included (JSON)
                Clients.Caller.passContact(u.UserID);
            }

           

        }


        public void SendFirstMessage(string message, List<string> contacts, string conversationName)
        {
            //Is user authorised to contact these Users??
            //carry out sanitation of all user input.

            //When conversation starts need to ensure group created and all participants are 
            //added

            Debug.WriteLine(message + "FIRST MESSAGE ");

            PersistStateHelper();

            if (message != null)
            {
                contacts.Add(AuthenticatedUser);
                Conversation con = StartConversation(contacts, conversationName);

                //creates new signalR group so that participants are
                //notified
                CreateNewGroup(con);

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

            //carry out sanitation of all user input.

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

            //Sanitise for display
            string htmlContent = System.Net.WebUtility.HtmlEncode(conversationMessage.Content);

            //updates all connected clients within that group. 
            Clients.Group(groupName).addNewMessageToPage(conversationMessage.User.UserID, htmlContent);

            //Also informs all users that they have received new message
            Clients.Group(groupName).newMessageNotification(con.ConversationID);
            
            Clients.Group(groupName).updateConversations();

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
          
           foreach (Conversation c in conversations)
           {
                Clients.Caller.AddExistingConversation(c);
           }

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