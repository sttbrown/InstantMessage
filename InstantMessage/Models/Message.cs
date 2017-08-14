using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Web.Security;

namespace InstantMessage.Models
{
    public class Message
    {

        public Message() {

            Sent = DateTime.Now;

        }

        //message has a string content, and date/time it was sent.
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }
        public string Content { get; set; }
        public DateTime Sent { get; set; }
        public Boolean Received { get; set; }

        //a message belongs to a single user
        //public string UserID { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

        //a message belongs to a single Conversation
        // public int ConversationID { get; set; } //not necessary?
        [JsonIgnore]
        public virtual Conversation Conversation { get; set; }  

    }

    //public class MessageDBContext : DbContext
    //{
    //    public DbSet<Message> Messages { get; set; }
    //}
}