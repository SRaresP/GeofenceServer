using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerExemplu.Data;

namespace ServerExemplu
{
    class ServerCore : ServerSocket
    {
        private Thread _thread = null;
        private bool _shouldRun = true;
        public void createServer(int port)
        {
            createSocket(port);
            _thread = new Thread(new ThreadStart(run));
            _thread.Start();
        }
        public void stopServer()
        {
            closeSocket();
        }

        private void run()
        {
            while (_shouldRun)
            {
                try
                {
                    Socket socket = acceptConnection();
                    if (socket == null)
                        return;

                    Console.WriteLine("Accepted connection from: " + socket.RemoteEndPoint);
                    ClientHandler clienthandler = new ClientHandler(socket, ClientDataStore.instance.clientCount);
                    clienthandler.initClient();
                    ClientDataStore.instance.addClient(clienthandler);
                }
                catch (Exception)
                {
                    return;
                }

                Thread.Yield();
            }
        }
    }
}
