using System;

namespace GeofenceServer.Data
{
    public class MissingConfigurationException : Exception
    {
        public MissingConfigurationException(string message) : base(message)
        {
        }
    }
}
