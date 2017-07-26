using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Security;

namespace InstantMessage.Models
{
    public class User
    {
       
        public User()
        {
            this.Conversations = new List<Conversation>();
            this.Messages = new List<Message>();
        }

        public User(string newUser) //from google authentication
        {
            this.UserID = newUser;
            this.Conversations = new List<Conversation>();
            this.Messages = new List<Message>();

        }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
   
        public ICollection<Connection> Connections { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
        public virtual ICollection<Message> Messages { get; set; }

    }

    //public class UserDBContext : DbContext
    //{
    //    public DbSet<User> Users { get; set; }
    //}
}