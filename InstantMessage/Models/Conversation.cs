using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace InstantMessage.Models
{
    /// <summary>
    /// Conversations have users, that participate in them and contain Users
    /// </summary>
    public class Conversation
    {
        public Conversation()
        {
            Messages = new List<Message>();
            Users = new List<User>();
            LastEdited = DateTime.Now.ToString("s"); 
        }

        //Conversation objects provide a means of grouping together messages
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationID { get; set; }

        public string Name { get; set; } //make this optional?
        public string LastMessage { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(200)]
        [Index("LastEdited")]
        public string LastEdited { get; set; }
        
        //allows conversations to be passed as JSON
        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set;  }
        
    }

}