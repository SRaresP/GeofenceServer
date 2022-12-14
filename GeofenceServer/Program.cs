using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GeofenceServer.Data;
using System.Diagnostics;
using System.Collections.Generic;

namespace GeofenceServer
{
    public class Program
    {
        private const int PORT = 8000;
        private static Socket mainSocket;
        private static Socket workerSocket;
        //Buffer size in bytes
        private const int BUFFER_SIZE = 1000;
        //COMM_SEPARATOR is currently 254 in ASCII
        //Used to separate the request type, user data,
        //location and other passed data from a received string message
        public const char COMM_SEPARATOR = '■';
        public const char USER_SEPARATOR = '√';
        public const char TRACKED_USERS_SEPARATOR = '°';

        //handleMsg cases
        private const string LOGIN_TARGET = "LOGIN_TARGET";
        private const string LOGIN_OVERSEER = "LOGIN_OVERSEER";
        private const string REGISTER_TARGET = "REGISTER_TARGET";
        private const string REGISTER_OVERSEER = "REGISTER_OVERSEER";
        private const string EDIT_TARGET = "EDIT_TARGET";
        private const string EDIT_OVERSEER = "EDIT_OVERSEER";
        private const string LOCATION_UPDATE_TARGET = "LOCATION_UPDATE_TARGET";
        private const string GET_UNIQUE_CODE_TARGET = "GET_UNIQUE_CODE_TARGET";
        private const string GET_USER = "GET_USER";
        private const string ADD_TARGET = "ADD_TARGET";

        //handleMsg results
        //positive
        private const string LOGGED_IN = "LOGGED_IN";
        private const string REGISTERED = "REGISTERED";
        private const string EDITED = "EDITED";
        private const string LOCATION_UPDATED = "LOCATION_UPDATED";
        private const string DELIVERED_CODE = "DELIVERED_CODE";
        private const string GOT_USER = "GOT_USER";
        private const string ADDED_TARGET = "ADDED_TARGET";
        //negative
        private const string NOT_FOUND = "NOT_FOUND";
        private const string WRONG_PASSWORD = "WRONG_PASSWORD";
        private const string EMAIL_ALREADY_TAKEN = "EMAIL_ALREADY_TAKEN";
        //code problem
        private const string UNDEFINED_CASE = "UNDEFINED_CASE";

        private class Case
		{
            public static string LoginTarget(string[] message)
            {
                using (TargetUserDbContext userdbContext = new TargetUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string email = userString[0];
                    var res = from users in userdbContext.Users
                              where users.Email == email
                              select users;
                    if (!res.Any())
                    {
                        return NOT_FOUND;
                    }
                    else
                    {
                        TargetUser user = new TargetUser();
                        foreach (TargetUser us in res)
                        {
                            user = us;
                        }
                        string password = userString[2];
                        string passwordHash = new CryptoHashHelper().GetHash(password);
                        if (user.PasswordHash != passwordHash)
                        {
                            return WRONG_PASSWORD;
                        }
                        else
                        {
                            return LOGGED_IN;
                        }
                    }
                }
            }
            public static string LoginOverseer(string[] message)
            {
                using (OverseerUserDbContext userdbContext = new OverseerUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string email = userString[0];
                    var res = from users in userdbContext.Users
                              where users.Email == email
                              select users;
                    if (!res.Any())
                    {
                        return NOT_FOUND;
                    }
                    else
                    {
                        OverseerUser user = new OverseerUser();
                        foreach (OverseerUser us in res)
                        {
                            user = us;
                        }
                        string password = userString[2];
                        string passwordHash = new CryptoHashHelper().GetHash(password);
                        if (user.PasswordHash != passwordHash)
                        {
                            return WRONG_PASSWORD;
                        }
                        else
                        {
                            return LOGGED_IN + COMM_SEPARATOR + user.Name + USER_SEPARATOR + user.TrackedUserIDs;
                        }
                    }
                }
            }
            public static string RegisterTarget(string[] message)
            {
                using (TargetUserDbContext userDbContext = new TargetUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string mail = userString[0];
                    var matches = from u in userDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        return EMAIL_ALREADY_TAKEN;
                    }
                    else
                    {
                        CryptoHashHelper crypto = new CryptoHashHelper();
                        userString[2] = crypto.GetHash(userString[2]);
                        TargetUser user = new TargetUser(userString[0],
                            userString[1],
                            userString[2]);
                        userDbContext.Users.Add(user);
                        userDbContext.SaveChanges();
                        return REGISTERED;
                    }
                }
            }
            public static string RegisterOverseer(string[] message)
            {
                using (OverseerUserDbContext userDbContext = new OverseerUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string mail = userString[0];
                    var matches = from u in userDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        return EMAIL_ALREADY_TAKEN;
                    }
                    else
                    {
                        CryptoHashHelper crypto = new CryptoHashHelper();
                        userString[2] = crypto.GetHash(userString[2]);
                        OverseerUser user = new OverseerUser(userString[0],
                            userString[1],
                            userString[2]);
                        userDbContext.Users.Add(user);
                        userDbContext.SaveChanges();
                        return REGISTERED;
                    }
                }
            }
            public static string EditTarget(string[] message)
			{
                using (TargetUserDbContext userDbContext = new TargetUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string mail = userString[0];
                    var matches = from u in userDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        TargetUser oldUser = new TargetUser();
                        foreach (TargetUser u in matches)
                        {
                            oldUser = u;
                        }
                        TargetUser newUser = new TargetUser(userString[0],
                            userString[1],
                            userString[2],
                            oldUser.NrOfCodeGenerations,
                            oldUser.LocationHistory);
                        userDbContext.Users.Remove(oldUser);
                        userDbContext.Users.Add(newUser);
                        userDbContext.SaveChanges();
                        return EDITED;
                    }
                    else
                    {
                        return NOT_FOUND;
                    }
                }
            }
            public static string EditOverseer(string[] message)
            {
                using (OverseerUserDbContext userDbContext = new OverseerUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string mail = userString[0];
                    var matches = from u in userDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        OverseerUser oldUser = new OverseerUser();
                        foreach (OverseerUser u in matches)
                        {
                            oldUser = u;
                        }
                        OverseerUser newUser = new OverseerUser(userString[0],
                            userString[1],
                            userString[2]);
                        userDbContext.Users.Remove(oldUser);
                        userDbContext.Users.Add(newUser);
                        userDbContext.SaveChanges();
                        return EDITED;
                    }
                    else
                    {
                        return NOT_FOUND;
                    }
                }
            }
            public static string LocationUpdateTarget(string[] message)
            {
                using (TargetUserDbContext tuDbContext = new TargetUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string locationString = message[2];
                    string mail = userString[0];
                    var matches = from u in tuDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        TargetUser user =  matches.First();
                        if (new CryptoHashHelper().GetHash(userString[2]) != user.PasswordHash)
						{
                            return WRONG_PASSWORD;
						}
                        user.LocationHistory = LocationHandler.AddLocation(user.LocationHistory, locationString);
                        tuDbContext.SaveChanges();
                        return LOCATION_UPDATED;
                    }
                    else
                    {
                        return NOT_FOUND;
                    }
                }
            }
            public static string GetUniqueCodeTarget(string[] message)
            {
                using (TargetUserDbContext tuDbContext = new TargetUserDbContext())
                {
                    string[] userString = message[1].Split(USER_SEPARATOR);
                    string mail = userString[0];
                    var matches = from u in tuDbContext.Users
                                  where u.Email == mail
                                  select u;
                    if (matches.Any())
                    {
                        TargetUser user = matches.First();
                        if (new CryptoHashHelper().GetHash(userString[2]) != user.PasswordHash)
                        {
                            return WRONG_PASSWORD;
                        }
                        return DELIVERED_CODE + COMM_SEPARATOR + TargetCodeHandler.Get(user);
                    }
                    else
                    {
                        return NOT_FOUND;
                    }
                }
            }
            public static string AddTarget(string[] message)
			{
                try
                {
                    string loginResult = LoginOverseer(message);
                    if (loginResult.StartsWith(LOGGED_IN))
                    {
                        int targetId = TargetCodeHandler.Validate(message[2]);
                        using (OverseerUserDbContext overseerUserDbContext = new OverseerUserDbContext())
                        {
                            string email = message[1].Split(USER_SEPARATOR)[0];
                            var res = from users in overseerUserDbContext.Users
                                      where users.Email == email
                                      select users;
                            if (!res.Any())
                            {
                                return NOT_FOUND;
                            }
                            else
							{
                                OverseerUser.AddTrackedUser(res.First().Id, targetId);
                                return ADDED_TARGET + COMM_SEPARATOR + targetId;
                            }
                        }
                    }
                    else
					{
                        return loginResult;

                    }
                }
                catch (KeyNotFoundException e)
                {
                    Trace.TraceWarning(e.Message);
                    Console.WriteLine(e.Message + "\n");
                    return NOT_FOUND;
                }
                catch (TargetCodeHandler.DuplicateCodesException e)
                {
                    Trace.TraceWarning(e.Message);
                    Console.WriteLine(e.Message + "\n");
                    // not found because the codes get deleted of duplicates are found
                    return NOT_FOUND;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.Message);
                    Console.WriteLine(e.Message + "\n");
                    return UNDEFINED_CASE;
                }
            }
            public static string GetUser(string[] message)
			{
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
				{
                    return loginResult;
				}

                int id = int.Parse(message[2]);

                using (TargetUserDbContext targetUserDbContext = new TargetUserDbContext())
                {
                    var res = from users in targetUserDbContext.Users
                              where users.Id == id
                              select users;
                    if (!res.Any())
                    {
                        return NOT_FOUND;
                    }

                    TargetUser targetUser = res.First();

                    return GOT_USER + COMM_SEPARATOR + targetUser.Id + USER_SEPARATOR + targetUser.Name + USER_SEPARATOR + targetUser.LocationHistory;
                }
            }
		}

        private static String ProcessRequest(String msg)
        {
            string[] message = msg.Split(Program.COMM_SEPARATOR);
            switch (message[0])
            {
                case LOGIN_TARGET:
                    return Case.LoginTarget(message);
                case LOGIN_OVERSEER:
                    return Case.LoginOverseer(message);
                case REGISTER_TARGET:
                    return Case.RegisterTarget(message);
                case REGISTER_OVERSEER:
                    return Case.RegisterOverseer(message);
                case EDIT_TARGET:
                    return Case.EditTarget(message);
                case EDIT_OVERSEER:
                    return Case.EditOverseer(message);
                case LOCATION_UPDATE_TARGET:
                    return Case.LocationUpdateTarget(message);
                case GET_UNIQUE_CODE_TARGET:
                    return Case.GetUniqueCodeTarget(message);
                case ADD_TARGET:
                    return Case.AddTarget(message);
                case GET_USER:
                    return Case.GetUser(message);
                default:
                    return UNDEFINED_CASE;
            }
        }

        private static void SendResponse(string msg)
        {
            byte[] bytesMsgRaspuns = Encoding.UTF8.GetBytes(msg);
            workerSocket.Send(bytesMsgRaspuns);
        }

        public static void Main(string[] args)
        {
            bool shouldRun = true;
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            mainSocket.Listen(10);

            TargetCodeHandler.Clear();

            Console.WriteLine("Initialisation finished, starting request processing.");

            while (shouldRun)
            {
                try
                {
                    //listen for a connection and accept it
                    workerSocket = mainSocket.Accept();
                    if (workerSocket == null)
                    {
                        return;
                    }
                    Console.WriteLine("\nAccepted connection from: " + workerSocket.RemoteEndPoint);
                    byte[] rawMsg = new byte[BUFFER_SIZE];
                    try
                    {
                        //read and check message
                        int bCount = workerSocket.Receive(rawMsg);
                        if (bCount > BUFFER_SIZE)
                        {
                            Trace.TraceError("Buffer overflow while reading from socket.");
                        }
                        string msg = Encoding.UTF8.GetString(rawMsg);
                        //process message and send a response
                        msg = msg.Split('\0')[0];
                        if (bCount > 0)
                        {
                            string[] splitMsg = msg.Split(COMM_SEPARATOR);
                            string request = splitMsg[0];
                            string from = splitMsg[1].Split(USER_SEPARATOR)[0];
                            Console.WriteLine("Request: " + request + " from:" + from);
                            string response = ProcessRequest(msg);
                            SendResponse(response);
                        }
                        workerSocket.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + "\n");
                        Trace.TraceError(ex.Message);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
