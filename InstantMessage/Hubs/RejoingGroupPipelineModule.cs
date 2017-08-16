using InstantMessage.DAL;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace InstantMessage
{
    public class RejoingGroupPipelineModule : HubPipelineModule
    {
        public override Func<HubDescriptor, IRequest, IList<string>, IList<string>>
            BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>>
            rejoiningGroups)
        {
            rejoiningGroups = (hb, r, l) =>
            {
                List<string> assignedGroups = new List<string>();
                using (var db = new InstantMessageContext())
                {
                    var user = db.Users.Include(u => u.Conversations)
                        .Single(u => u.UserID == r.User.Identity.Name);
                    foreach (var item in user.Conversations)
                    {
                        assignedGroups.Add(item.ConversationID.ToString());
                        Debug.WriteLine(item.ConversationID.ToString());
                    }
                }
                return assignedGroups;
            };

            return rejoiningGroups;
        }
    }
}
