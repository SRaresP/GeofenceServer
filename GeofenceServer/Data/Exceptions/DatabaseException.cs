using System;

namespace GeofenceServer.Data
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }
    }
}
