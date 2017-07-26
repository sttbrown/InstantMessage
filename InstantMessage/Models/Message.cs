﻿using System;
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

            //MembershipUser user = Membership.GetUser();
            //UserID = user.ToString();

        }

        //message has a string content, and date/time it was sent.
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }

        public string Content { get; set; }
        //public DateTime Sent { get; set; }

        //a message belongs to a single user
        //public string UserID { get; set; }
        public virtual User User { get; set; }

        //a message belongs to a single Conversation
       // public int ConversationID { get; set; } //not necessary?
        public virtual Conversation Conversation { get; set; }  

    }

    //public class MessageDBContext : DbContext
    //{
    //    public DbSet<Message> Messages { get; set; }
    //}
}