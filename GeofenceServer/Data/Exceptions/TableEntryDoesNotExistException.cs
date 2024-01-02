using System;

namespace GeofenceServer.Data
{
    public class TableEntryDoesNotExistException : Exception
    {
        public TableEntryDoesNotExistException(string message) : base(message)
        {
        }
    }
}
