using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace InstantMessage.Models
{
    public class Conversation
    {
        public Conversation()
        {
            Messages = new List<Message>();
            Users = new List<User>();
            LastEdited = DateTime.Now;
        }

        //Conversation objects provide a means of grouping together messages
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationID { get; set; }

        public string Name { get; set; } //make this optional?
        public string LastMessage { get; set; }

        //Is this a good idea? seems as though it might be 
        //[Index("LastEdited")]
        public DateTime LastEdited { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set;  }
        
    }

    //public class ConversationDBContext : DbContext
    //{
    //    public DbSet<Conversation> Conversations { get; set; }
    //}


}