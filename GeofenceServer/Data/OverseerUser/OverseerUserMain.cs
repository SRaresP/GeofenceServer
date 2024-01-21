using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class OverseerUser : DatabaseClient
    {

        public OverseerUser() : base()
        {
            Id = DEFAULT_ID;
            Email = "";
            Name = "";
            PasswordHash = "";
            TrackedUserIds = Enumerable.Repeat(TrackedUserId.DEFAULT_ID, 10).ToArray();
        }
        public OverseerUser(OverseerUser toCopy) : base(toCopy) { }

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
            if (Id == DEFAULT_ID)
            {
                throw new TableEntryDoesNotExistException($"Id of overseer was {TrackedUserId.DEFAULT_ID}.");
            }
            TrackedUserIds = TrackedUserId.GetByOverseerId(Id);
        }
    }
}
