using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace InstantMessage.Models
{
    public class Conversation
    {
        public Conversation()
        {
            Messages = new List<Message>();
            Users = new List<User>();
        }

        //Conversation objects provide a means of grouping together messages
        public int ConversationID { get; set; }
        public string Name { get; set; } //make this optional?
        
        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<User> Users { get; set;  }


        //other candidates/properties
        //public DateTime DateStarted { get; set; }
        // public virtual ICollection<Participant> Participants { get; set; }
    }

    //public class ConversationDBContext : DbContext
    //{
    //    public DbSet<Conversation> Conversations { get; set; }
    //}
}