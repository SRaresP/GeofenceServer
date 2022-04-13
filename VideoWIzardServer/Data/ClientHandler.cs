using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerExemplu.Data;

namespace ServerExemplu.Data
{
    class ClientHandler
    {
        private const char _sepparator = '~';
        private Socket _sk = null;
        private int _idx = -1;
        private Thread _th = null;
        private bool _shouldRun = true;
        private bool _isRunning = true;
        public ClientHandler(Socket sk, int id)
        {
            _sk = sk;
            _idx = id;
        }

        public void initClient()
        {
            if (null != _th)
                return;

            _th = new Thread(new ThreadStart(run));
            _th.Start();
        }

        public void stopClient()
        {
            if (_th == null )
                return;

            _sk.Close();
            _shouldRun = false;
        }

        public bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(10000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        private void handleMsg(String msg)
        {
            string[] message = msg.Split(_sepparator);
            switch(message[0])
            {
                case "Login":
                    using (UserDbContext userdbContext = new UserDbContext())
                    {
                        string email = message[1];
                        var res = from users in userdbContext.Users
                                  where users.Email == email
                                  select users;
                        if (!res.Any())
                        {
                            Console.WriteLine("Email nu a fost gasit la logare.");
                            sendResponse("NOTFOUND");
                        }
                        else
                        {
                            User user = new User();
                            foreach (User us in res)
                            {
                                user = us;
                            }
                            string password = message[2].Split('\0')[0];
                            string passwordHash = new CryptoHashHelper().GetHash(password);
                            if (user.PasswordHash != passwordHash)
                            {
                                Console.WriteLine("Wrong password for " + msg);
                                sendResponse("WRONGPASSWORD");
                            }
                            else
                            {
                                sendResponse("PROCEED" + User.Separator + user.ToString());
                                Console.WriteLine("Sent PROCEED" + User.Separator + user.ToString());
                            }
                        }
                    }
                    break;
                case "Register":
                    using (UserDbContext userDbContext = new UserDbContext())
                    {
                        string mail = message[1];
                        var matches = from u in userDbContext.Users
                                      where u.Email == mail
                                      select u;
                        if (matches.Any())
                        {
                            Console.WriteLine("Cannot create account, user Email is already in database.");
                            sendResponse("ALREADYEXISTS");
                        }
                        else
                        {
                            bool payment = message[4] == "1";
                            CryptoHashHelper crypto = new CryptoHashHelper();
                            message[3] = crypto.GetHash(message[3]);
                            User user = new User(message[1],
                                message[2],
                                message[3],
                                payment);
                            userDbContext.Users.Add(user);
                            userDbContext.SaveChanges();
                            sendResponse("PROCEED" + User.Separator + user.ToString());
                            Console.WriteLine("Sent PROCEED" + User.Separator + user.ToString());
                        }
                    }
                    break;
                case "Edit":
                    using (UserDbContext userDbContext = new UserDbContext())
                    {
                        string mail = message[1];
                        var matches = from u in userDbContext.Users
                                      where u.Email == mail
                                      select u;
                        if (matches.Any())
                        {
                            bool payment = message[5].Split('\0')[0].Equals("1");
                            User newUser = new User(message[2],
                                message[3],
                                message[4],
                                payment);
                            User oldUser = new User();
                            foreach(User u in matches)
                            {
                                oldUser = u;
                            }
                            userDbContext.Users.Remove(oldUser);
                            userDbContext.Users.Add(newUser);
                            userDbContext.SaveChanges();
                            sendResponse("PROCEED" + User.Separator + newUser.ToString());
                            Console.WriteLine("Sent PROCEED" + User.Separator + newUser.ToString());
                        }
                        else
                        {
                            sendResponse("NOTFOUND");
                        }
                    }

                    break;
                default:
                    break;
            }
        }
        private void sendResponse(String msg)
        {
            byte[] bytesMsgRaspuns = Encoding.ASCII.GetBytes(msg);
            _sk.Send(bytesMsgRaspuns);
        }
        
        private void run()
        {
            

            while (_shouldRun)
            {
                //Console.WriteLine("Client... "+_idx);
                // Attention! This is the largest message one can receive!
                byte[] rawMsg = new byte[500];
                try
                {
                    
                        int bCount = _sk.Receive(rawMsg);
                        String msg = Encoding.UTF8.GetString(rawMsg);
                    if (bCount > 0)
                    {
                        Console.WriteLine("Client " + _idx + ": " + msg);
                        handleMsg(msg);
                    }
                                     
                        
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(1);
            }
            _isRunning = false;
            
        }

        public bool isAlive()
        {
            return _isRunning;
        }
    }
}
