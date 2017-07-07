
using InstantMessage.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace InstantMessage.DAL
{
    public class InstantMessageContext : DbContext
    {

        public InstantMessageContext() : base("InstantMessageContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        //public DbSet<Participant> Participants { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //Include or omit this line to pluralise table names
        }
    }

}