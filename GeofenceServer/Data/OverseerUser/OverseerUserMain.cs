using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class OverseerUser : DatabaseClient
    {
        public OverseerUser(string Email, string Name, string PasswordHash) : base()
        {
            this.Id = -1;
            this.Email = Email;
            this.Name = Name;
            this.PasswordHash = PasswordHash;
            TrackedUserIds = Enumerable.Repeat(TrackedUserId.DEFAULT_ID, 10).ToArray();
        }

        public OverseerUser() : base()
        {
            Id = -1;
            Email = "";
            Name = "";
            PasswordHash = "";
            TrackedUserIds = Enumerable.Repeat(TrackedUserId.DEFAULT_ID, 10).ToArray();
        }

        public bool AddTrackedUser(long targetId)
        {
            return ArrayHelper<long>.Add(TrackedUserIds, targetId, TrackedUserId.DEFAULT_ID);
        }

        public bool RemoveTrackedUser(long targetId)
        {
            return ArrayHelper<long>.Remove(TrackedUserIds, targetId, TrackedUserId.DEFAULT_ID);
        }

        public void LoadTrackedUserIds()
        {
            if (Id == -1)
            {
                throw new TableEntryDoesNotExistException($"Id of overseer was {TrackedUserId.DEFAULT_ID}.");
            }
            TrackedUserIds = TrackedUserId.GetByOverseerId(Id);
        }
    }
}
