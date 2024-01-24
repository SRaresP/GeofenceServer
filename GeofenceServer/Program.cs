using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GeofenceServer.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeofenceServer
{
    public class Program
    {
        private const int PORT = 8000;
        private static Socket mainSocket;
        private static Socket workerSocket;

        private static string END = "<_-END-_>";

        //Buffer size in bytes
        private const int BUFFER_SIZE = 1000;

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
        private const string REMOVE_TARGET = "REMOVE_TARGET";
        private const string GET_SETTINGS = "GET_SETTINGS";
        private const string CHANGE_SETTINGS = "CHANGE_SETTINGS";
        private const string REMOVE_SETTINGS = "REMOVE_SETTINGS";
        private const string GET_TARGET_LOCATION_AND_INTERVAL = "GET_TARGET_LOCATION_AND_INTERVAL";
        private const string GET_GEOAREAS = "GET_GEOAREAS";
        private const string ADD_GEOAREA = "ADD_GEOAREA";

        //handleMsg results
        //positive
        private const string LOGGED_IN = "LOGGED_IN";
        private const string REGISTERED = "REGISTERED";
        private const string EDITED = "EDITED";
        private const string LOCATION_UPDATED = "LOCATION_UPDATED";
        private const string DELIVERED_CODE = "DELIVERED_CODE";
        private const string GOT_USER = "GOT_USER";
        private const string ADDED_TARGET = "ADDED_TARGET";
        private const string REMOVED_TARGET = "REMOVED_TARGET";
        private const string GOT_SETTINGS = "GOT_SETTINGS";
        private const string CHANGED_SETTINGS = "CHANGED_SETTINGS";
        private const string REMOVED_SETTINGS = "REMOVED_SETTINGS";
        private const string GOT_TARGET_LOCATION_AND_INTERVAL = "GOT_TARGET_LOCATION_AND_INTERVAL";
        private const string GOT_GEOAREAS = "GOT_GEOAREAS";
        private const string ADDED_GEOAREA = "ADDED_GEOAREA";
        //negative
        private const string NOT_FOUND = "NOT_FOUND";
        private const string WRONG_PASSWORD = "WRONG_PASSWORD";
        private const string EMAIL_ALREADY_TAKEN = "EMAIL_ALREADY_TAKEN";
        private const string COULD_NOT_REMOVE_TARGET = "COULD_NOT_REMOVE_TARGET";
        private const string NOT_A_TARGET_ID = "NOT_A_TARGET_ID";
        private const string NOT_AN_INTERVAL = "NOT_AN_INTERVAL";
        private const string NOT_A_GEOAREA = "NOT_A_GEOAREA";
        private const string ALREADY_TRACKING = "ALREADY_TRACKING";
        //code problem
        private const string UNDEFINED_CASE = "UNDEFINED_CASE";

        private class Case
		{
            public static string LoginTarget(string[] message)
            {
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];
                TargetUser user = new TargetUser();
                user.Email = email;
                user.LoadUsingAvailableData();
                if (!user.IsLoaded())
                {
                    return NOT_FOUND;
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
            public static string LoginOverseer(string[] message)
            {
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];
                OverseerUser user = new OverseerUser();
                user.Email = email;
                user.LoadUsingAvailableData();
                if (!user.IsLoaded())
                {
                    return NOT_FOUND;
                }
                string password = userString[2];
                string passwordHash = new CryptoHashHelper().GetHash(password);
                if (user.PasswordHash != passwordHash)
                {
                    return WRONG_PASSWORD;
                }
                else
                {
                    return LOGGED_IN + COMM_SEPARATOR + user.Name + USER_SEPARATOR + String.Join(TRACKED_USERS_SEPARATOR.ToString(), user.TrackedUserIds);
                }
            }
            public static string RegisterTarget(string[] message)
            {
                string[] userString = message[1].Split(USER_SEPARATOR);
                string mail = userString[0];
                TargetUser user = new TargetUser();
                user.Email = mail;
                user.LoadUsingAvailableData();
                if (user.IsLoaded())
                {
                    return EMAIL_ALREADY_TAKEN;
                }
                CryptoHashHelper crypto = new CryptoHashHelper();
                user = new TargetUser()
                {
                    Email = userString[0],
                    Name = userString[1],
                    PasswordHash = crypto.GetHash(userString[2])
                };
                user.Save();
                return REGISTERED;
            }
            public static string RegisterOverseer(string[] message)
            {
                string[] userString = message[1].Split(USER_SEPARATOR);
                string mail = userString[0];
                OverseerUser user = new OverseerUser();
                user.Email = mail;
                user.LoadUsingAvailableData();
                if (user.IsLoaded())
                {
                    return EMAIL_ALREADY_TAKEN;
                }
                CryptoHashHelper crypto = new CryptoHashHelper();
                userString[2] = crypto.GetHash(userString[2]);
                user.Name = userString[1];
                user.PasswordHash = userString[2];
                user.Save();
                // check save
                return REGISTERED;
            }
            public static string EditTarget(string[] message)
			{
                throw new NotImplementedException();
            }
            public static string EditOverseer(string[] message)
            {
                throw new NotImplementedException();
            }
            public static string LocationUpdateTarget(string[] message)
            {
                long interval;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string locationString = message[2];
                string mail = userString[0];
                TargetUser user = new TargetUser();
                user.Email = mail;
                user.LoadUsingAvailableData();
                if (!user.IsLoaded())
                {
                    return NOT_FOUND;
                }
                if (new CryptoHashHelper().GetHash(userString[2]) != user.PasswordHash)
				{
                    return WRONG_PASSWORD;
				}
                user.LocationHistory = LocationHandler.AddLocation(user.LocationHistory, locationString);
                user.Save();
                List<Dictionary<string, object>> result = TrackingSettings.ExecuteQuery("SELECT * " +
                    $"FROM {TrackingSettings.TableName} " +
                    $"WHERE target_id = {user.Id} " +
                    $"ORDER BY `interval` ASC " +
                    $"LIMIT 1;");
                if (result.Count() < 1)
                {
                    interval = TrackingSettings.DEFAULT_INTERVAL;
                }
                else
                {
                    long.TryParse(result[0]["interval"].ToString(), out interval);
                }
                return LOCATION_UPDATED + COMM_SEPARATOR + interval;
            }
            public static string GetUniqueCodeTarget(string[] message)
            {
                string[] userString = message[1].Split(USER_SEPARATOR);
                string mail = userString[0];
                TargetUser user = new TargetUser();
                user.Email = mail;
                user.LoadUsingAvailableData();
                if (!user.IsLoaded())
                {
                    return NOT_FOUND;
                }
                if (new CryptoHashHelper().GetHash(userString[2]) != user.PasswordHash)
                {
                    return WRONG_PASSWORD;
                }
                return DELIVERED_CODE + COMM_SEPARATOR + TargetCodeHandler.Get(user);
            }
            public static string AddTarget(string[] message)
			{
                try
                {
                    string loginResult = LoginOverseer(message);
                    if (loginResult.StartsWith(LOGGED_IN))
                    {
                        long targetId = TargetCodeHandler.Validate(message[2]);
                        string email = message[1].Split(USER_SEPARATOR)[0];
                        OverseerUser overseer = new OverseerUser();
                        overseer.Email = email;
                        overseer.LoadUsingAvailableData();
                        if (!overseer.IsLoaded())
                        {
                            return NOT_FOUND;
                        }
                        if (overseer.TrackedUserIds.Contains(targetId)) {
                            return ALREADY_TRACKING + COMM_SEPARATOR + targetId;
                        }
                        else {
                            if (!overseer.AddTrackedUser(targetId))
                            {
                                return UNDEFINED_CASE;
                            }
                            overseer.Save();
                            return ADDED_TARGET + COMM_SEPARATOR + targetId;
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
                    return NOT_FOUND;
                }
                catch (TargetCodeHandler.DuplicateCodesException e)
                {
                    Trace.TraceWarning(e.Message);
                    // not found because the codes get deleted if duplicates are found
                    return NOT_FOUND;
                }
            }
            public static string GetUser(string[] message)
			{
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
				{
                    return loginResult;
				}

                long overseerId, targetId, interval;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                OverseerUser overseer = new OverseerUser();
                overseer.Email = email;
                overseer.LoadUsingAvailableData();
                if (!overseer.IsLoaded())
                {
                    throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                }
                overseerId = overseer.Id;

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }

                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerId,
                    TargetId = targetId
                };
                settings.LoadUsingAvailableData();
                // Interval will be DEFAULT_INTERVAL regardless if it's successfully loaded or not.
                interval = settings.Interval;

                TargetUser targetUser = new TargetUser();
                targetUser.Id = targetId;
                targetUser.LoadUsingAvailableData();
                if (!overseer.IsLoaded())
                {
                    return NOT_FOUND;
                }

                return GOT_USER +
                    COMM_SEPARATOR +
                    targetUser.Id +
                    USER_SEPARATOR +
                    targetUser.Name +
                    USER_SEPARATOR +
                    LocationHandler.truncateHistoryForTransmission(targetUser.LocationHistory) +
                    USER_SEPARATOR +
                    interval;
            }
            public static string RemoveTarget(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }

                string[] userData = message[1].Split(USER_SEPARATOR);
                string userEmail = userData[0];
                OverseerUser overseerUser = new OverseerUser();
                overseerUser.Email = userEmail;
                overseerUser.LoadUsingAvailableData();
                if (!overseerUser.IsLoaded())
                {
                    throw new TableEntryDoesNotExistException($"Failed to load {overseerUser.GetType().Name} after the login for it was successful. Check this ASAP.");
                }

                string id = message[2].Trim();
                int targetId = int.Parse(id);
                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerUser.Id,
                    TargetId = targetId
                };
                settings.Delete();
                GeoArea.ExecuteNonQuery($"DELETE FROM {GeoArea.TableName}" +
                    " WHERE overseer_id = " + overseerUser.Id + " AND " +
                    "target_id = " + targetId);
                if (!overseerUser.RemoveTrackedUser(targetId))
                {
                    return COULD_NOT_REMOVE_TARGET;
                }
                overseerUser.Save();
                return REMOVED_TARGET;
            }
            public static string GetSettings(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }

                long overseerId, targetId;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }

                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerId,
                    TargetId = targetId
                };
                settings.LoadUsingAvailableData();
                return GOT_SETTINGS + COMM_SEPARATOR + settings.Interval;
            }
            public static string ChangeSettings(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }

                long overseerId, targetId;
                int interval;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }
                try
                {
                    interval = int.Parse(message[3]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_AN_INTERVAL;
                }

                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerId,
                    TargetId = targetId
                };
                settings.Interval = interval;
                settings.Save();
                return CHANGED_SETTINGS;
            }
            public static string RemoveSettings(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }

                long overseerId, targetId;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }

                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerId,
                    TargetId = targetId
                };
                settings.Delete();
                return REMOVED_SETTINGS;
            }
            public static string GetTargetLocationAndInterval(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }

                long overseerId, targetId;
                int interval;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }
                TargetUser target = new TargetUser();
                target.Id = targetId;
                target.LoadUsingAvailableData();
                if (!target.IsLoaded())
                {
                    return NOT_FOUND;
                }
                TrackingSettings settings = new TrackingSettings()
                {
                    OverseerId = overseerId,
                    TargetId = targetId
                };
                settings.LoadUsingAvailableData();
                interval = settings.Interval;
                return GOT_TARGET_LOCATION_AND_INTERVAL +
                    COMM_SEPARATOR +
                    LocationHandler.truncateHistoryForTransmission(target.LocationHistory) +
                    COMM_SEPARATOR +
                    interval;
            }
            public static string GetGeoAreas(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }
                long overseerId, targetId;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }

                string geoAreas = GeoArea.GetGeoAreaStr(overseerId, targetId);

                return GOT_GEOAREAS +
                    COMM_SEPARATOR +
                    geoAreas;
            }
            public static string AddGeoArea(string[] message)
            {
                string loginResult = LoginOverseer(message);
                if (!loginResult.StartsWith(LOGGED_IN))
                {
                    return loginResult;
                }
                long overseerId, targetId;
                string[] userString = message[1].Split(USER_SEPARATOR);
                string email = userString[0];

                {
                    OverseerUser overseer = new OverseerUser();
                    overseer.Email = email;
                    overseer.LoadUsingAvailableData();
                    if (!overseer.IsLoaded())
                    {
                        throw new TableEntryDoesNotExistException($"Failed to load {overseer.GetType().Name} after the login for it was successful. Check this ASAP.");
                    }
                    overseerId = overseer.Id;
                }

                try
                {
                    targetId = int.Parse(message[2]);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return NOT_A_TARGET_ID;
                }
                try
                {
                    // For now only one GeoArea will be allotted to overseer-target pairs
                    // Translation: delete all GeoFences associated to this OverseerUser
                    int affectedRows = GeoFence.ExecuteNonQuery($"DELETE FROM {GeoFence.TableName}" +
                        " WHERE geo_area_id = (" +
                        "   SELECT id " +
                        $"  FROM {GeoArea.TableName} " +
                        $"  WHERE overseer_id = {overseerId} " +
                        $"  LIMIT 1" +
                        ")");
                    // Delete the pair's GeoArea
                    affectedRows = GeoArea.ExecuteNonQuery($"DELETE FROM {GeoArea.TableName}" +
                        " WHERE overseer_id = " + overseerId + " AND " +
                        "target_id = " + targetId);
                    GeoArea geoArea = new GeoArea(message[3], overseerId, targetId);
                    geoArea.Save();
                    foreach (GeoFence geoFence in geoArea.GeoFences)
					{
                        geoFence.GeoAreaId = geoArea.Id;
                        geoFence.Save();
					}
                    return ADDED_GEOAREA;
                }
                catch (Exception e)
                    when (e is ArgumentNullException ||
                    e is FormatException ||
                    e is OverflowException)
				{
                    return NOT_A_GEOAREA;
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
                case REMOVE_TARGET:
                    return Case.RemoveTarget(message);
                case GET_SETTINGS:
                    return Case.GetSettings(message);
                case CHANGE_SETTINGS:
                    return Case.ChangeSettings(message);
                case REMOVE_SETTINGS:
                    return Case.RemoveSettings(message);
                case GET_TARGET_LOCATION_AND_INTERVAL:
                    return Case.GetTargetLocationAndInterval(message);
                case GET_GEOAREAS:
                    return Case.GetGeoAreas(message);
                case ADD_GEOAREA:
                    return Case.AddGeoArea(message);
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
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            mainSocket.Listen(10);

            TargetCodeHandler.Clear();

            Trace.TraceInformation("Initialisation finished, starting request processing.");

            while (true)
            {
                try
                {
                    workerSocket = mainSocket.Accept();
                    if (workerSocket == null)
                    {
                        continue;
                    }

                    Trace.TraceInformation("Accepted connection from: " + workerSocket.RemoteEndPoint);

                    byte[] rawMsg = new byte[BUFFER_SIZE];
                    string message = "";

                    int bCount = workerSocket.Receive(rawMsg);

                    while (bCount != 0)
                    {
                        message += Encoding.UTF8.GetString(rawMsg);
                        if (message.Contains(END))
                        {
                            break;
                        }
                        bCount = workerSocket.Receive(rawMsg);
                    }

                    int indexOfNull = message.IndexOf('\0');
                    if (indexOfNull >= 0) {
                        message = message.Remove(indexOfNull, message.Length - indexOfNull);
                    }

                    string msg = message.Substring(0, message.IndexOf(END));
                    string[] splitMsg = msg.Split(COMM_SEPARATOR);
                    string request = splitMsg[0];
                    string from = splitMsg[1].Split(USER_SEPARATOR)[0];

                    Trace.TraceInformation("Request: " + request + " from " + from);

                    string response = ProcessRequest(msg);
                    SendResponse(response + END);

                    int lastIndexOfCommSep = response.IndexOf(COMM_SEPARATOR);
                    if (lastIndexOfCommSep == -1)
                    {
                        lastIndexOfCommSep = response.Length;
                    }
                    Trace.TraceInformation("Response: " + response.Substring(0, lastIndexOfCommSep));

                    workerSocket.Close();
                }
                catch (Exception e)
                {
                    workerSocket.Close();
                    Trace.TraceError(e.ToString());
                }
            }
        }
    }
}
