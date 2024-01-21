using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeofenceServer.Data
{
    public partial class TrackedUserId : DatabaseClient
    {
        public TrackedUserId() : base() {
            TargetId = DEFAULT_ID;
            OverseerId = DEFAULT_ID;
        }
        public TrackedUserId(TrackedUserId toCopy) : base(toCopy) { }

        public static void CleanupByOverseer(OverseerUser overseer)
        {
            string trackedUserIds = String.Join(", ", overseer.TrackedUserIds);
            ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE overseer_id = {overseer.Id} AND " +
                $"target_id NOT IN ({trackedUserIds});");
        }

        public static void SyncOverseer(OverseerUser overseer)
        {
            CleanupByOverseer(overseer);
            AddOverseersTrackedUserIds(overseer);
        }

        public static long[] GetByOverseerId(long overseerId)
        {
            if (overseerId == DEFAULT_ID) 
            {
                throw new ArgumentException($"OverseerId was {DEFAULT_ID}.");
            }
            List<Dictionary<string, object>> results = ExecuteQuery($"SELECT * " +
                $"FROM {TableName} " +
                $"WHERE overseer_id = {overseerId};");
            long[] trackedUserIds = Enumerable.Repeat(DEFAULT_ID, 10).ToArray();
            for (int index = 0; index < results.Count && index < trackedUserIds.Count(); ++index)
            {
                trackedUserIds[index] = Convert.ToInt64(results[index]["target_id"]);
            }

            return trackedUserIds;
        }

        public static long[] GetByTargetId(int targetId)
        {
            if (targetId == DEFAULT_ID)
            {
                throw new ArgumentException($"TargetId was {DEFAULT_ID}.");
            }
            List<Dictionary<string, object>> results = ExecuteQuery($"SELECT * " +
                $"FROM {TableName} " +
                $"WHERE target_id = {targetId};");
            long[] trackedUserIds = new long[results.Count];
            for (int index = 0; index < results.Count; ++index)
            {
                trackedUserIds[index] = (long)results[index]["target_id"];
            }

            return trackedUserIds;
        }

        public static void DeleteByOverseer(long overseerId)
        {
            int result = ExecuteNonQuery($"DELETE FROM {TableName} " +
                $"WHERE overseer_id = {overseerId}");
        }

        public static void AddOverseersTrackedUserIds(OverseerUser overseer)
        {
            if (overseer == null)
            {
                throw new ArgumentException("Passed OverseerUser was null.");
            }
            if (overseer.TrackedUserIds == null)
            {
                throw new ArgumentException("OverseerUser's TrackedUserIds list was null.");
            }
            if (overseer.TrackedUserIds.Length == 0)
            {
                throw new ArgumentException("OverseerUser's TrackedUserIds list was empty.");
            }
            // INSERT IGNORE inserts a new record if it doesn't already exist in this case
            string sql = $"INSERT IGNORE INTO {TableName} (overseer_id, target_id) ";
            int index = 0;
            bool runQuery = false;
            for (; index < overseer.TrackedUserIds.Length; ++index)
            {
                if (overseer.TrackedUserIds[index] != DEFAULT_ID)
                {
                    sql += $"VALUES ({overseer.Id}, {overseer.TrackedUserIds[0]}) ";
                    runQuery = true;
                }
            }
            for (; index < overseer.TrackedUserIds.Length; ++index)
            {
                if (overseer.TrackedUserIds[index] != DEFAULT_ID)
                {
                    sql += $", " +
                        $"({overseer.Id}, {overseer.TrackedUserIds[index]})";
                    runQuery = true;
                }
            }
            if (runQuery)
            {
                sql += ';';
                int result = ExecuteNonQuery(sql);
                if (result < 1)
                {
                    throw new DatabaseException($"Failed to add TrackedUserId (overseer_id = {overseer.Id}) to database.");
                }
            }
        }
    }
}
