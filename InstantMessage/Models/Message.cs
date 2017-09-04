using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Web.Security;

namespace InstantMessage.Models
{
    /// <summary>
    /// Model object representing a message
    /// </summary>
    public class Message
    {
        public Message()
        {
            Sent = DateTime.Now.ToString("g");
        }

        //message has a string content, and date/time it was sent.
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }
        public string Content { get; set; }
        public string Sent { get; set; }
        public Boolean Received { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual Conversation Conversation { get; set; }  

    }
}