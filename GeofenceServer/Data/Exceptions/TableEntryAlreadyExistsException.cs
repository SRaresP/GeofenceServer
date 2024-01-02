using System;

namespace GeofenceServer.Data
{
    public class TableEntryAlreadyExistsException : Exception
    {
        public TableEntryAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
