using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GeofenceServer.Data;
using System.Diagnostics;

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

        //handleMsg cases
        private const string LOGIN = "LOGIN";
        private const string REGISTER = "REGISTER";
        private const string EDIT = "EDIT";
        private const string LOCATION_UPDATE = "LOCATION_UPDATE";
        private const string GET_UNIQUE_CODE = "GET_UNIQUE_CODE";

        //handleMsg results
        //positive
        private const string LOGGED_IN = "LOGGED_IN";
        private const string REGISTERED = "REGISTERED";
        private const string EDITED = "EDITED";
        private const string LOCATION_UPDATED = "LOCATION_UPDATED";
        private const string DELIVERED_CODE = "DELIVERED_CODE";
        //negative
        private const string NOT_FOUND = "NOT_FOUND";
        private const string WRONG_PASSWORD = "WRONG_PASSWORD";
        private const string EMAIL_ALREADY_TAKEN = "EMAIL_ALREADY_TAKEN";
        //code problem
        private const string UNDEFINED_CASE = "UNDEFINED_CASE";

        private class Case
		{
            public static string Login(string[] message)
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
            public static string Register(string[] message)
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
            public static string Edit(string[] message)
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
            public static string LocationUpdate(string[] message)
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
            public static string GetUniqueCode(string[] message)
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
                        mail += user.NrOfCodeGenerations;
                        int code = Math.Abs(mail.GetHashCode() % 100000000);
                        ++user.NrOfCodeGenerations;
                        tuDbContext.SaveChanges();
                        String toReturn = DELIVERED_CODE + COMM_SEPARATOR + code;
                        return toReturn;
                    }
                    else
                    {
                        return NOT_FOUND;
                    }
                }
            }
		}

        private static String handleMsg(String msg)
        {
            string[] message = msg.Split(Program.COMM_SEPARATOR);
            switch (message[0])
            {
                case LOGIN:
                    return Case.Login(message);
                case REGISTER:
                    return Case.Register(message);
                case EDIT:
                    return Case.Edit(message);
                case LOCATION_UPDATE:
                    return Case.LocationUpdate(message);
                case GET_UNIQUE_CODE:
                    return Case.GetUniqueCode(message);
                default:
                    return UNDEFINED_CASE;
            }
        }

        private static void sendResponse(String msg)
        {
            byte[] bytesMsgRaspuns = Encoding.UTF8.GetBytes(msg);
            workerSocket.Send(bytesMsgRaspuns);
        }

        public static void Main(String[] args)
        {
            bool shouldRun = true;
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            mainSocket.Listen(10);

            while (shouldRun)
            {
                try
                {
                    workerSocket = mainSocket.Accept();
                    if (workerSocket == null)
                    {
                        return;
                    }
                    Console.WriteLine("Accepted connection from: " + workerSocket.RemoteEndPoint);
                    byte[] rawMsg = new byte[BUFFER_SIZE];
                    try
                    {
                        int bCount = workerSocket.Receive(rawMsg);
                        if (bCount > BUFFER_SIZE)
                        {
                            Trace.TraceError("Buffer overflow while reading from socket.");
                        }
                        String msg = Encoding.UTF8.GetString(rawMsg);
                        msg = msg.Split('\0')[0];
                        if (bCount > 0)
                        {
                            Console.WriteLine("Client: " + msg);
                            string response = handleMsg(msg);
                            sendResponse(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
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
