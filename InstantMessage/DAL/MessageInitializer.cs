using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using InstantMessage.Models;

namespace InstantMessage.DAL    
{
    public class MessageInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<InstantMessageContext>
    {
        protected override void Seed(InstantMessageContext context)
        {
            var user = new List<User>
            {
                new User{UserID="1",UserName="GiantSteps", FirstName="John", LastName="Coltrane"},
                new User{UserID="2",UserName="BirthOfCool", FirstName="Miles", LastName="Davis"},

            };
            user.ForEach(u => context.Users.Add(u));
            context.SaveChanges();
            var conversation = new List<Conversation>
            {
                new Conversation{Name="best horn player", LastEdited=DateTime.Now.ToString("g"), LastMessage ="Example Last Message" },
                new Conversation{Name="wheres the piano?",  LastEdited=DateTime.Now.ToString("g"), LastMessage="Example Last Message" },

            };     
            conversation.ForEach(c => context.Conversations.Add(c));
            context.SaveChanges();
            var messages = new List<Message>
            {
            new Message{MessageID= 1, Content="Charlie is the finest horn player"},
            new Message{MessageID= 2, Content="what you mean Bird?" },
            };

            messages.ForEach(m => context.Messages.Add(m));
            context.SaveChanges();


        }
    }
}