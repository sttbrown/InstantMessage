using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Security;

namespace InstantMessage.Models
{
    /// <summary>
    /// Model representing a message application user
    /// </summary>
    public class User
    {
        public User()
        {
            this.Conversations = new List<Conversation>();
            this.Messages = new List<Message>();
        }

        public User(string newUser) //from OWIN authentication
        {
            this.UserID = newUser;
            this.Conversations = new List<Conversation>();
            this.Messages = new List<Message>();
            this.LastActive = DateTime.Now.ToString("g");

        }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastActive { get; set; }

        [JsonIgnore]
        public virtual ICollection<Conversation> Conversations { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }

    }
}