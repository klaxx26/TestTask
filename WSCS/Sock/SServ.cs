using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace WSCS
{
    public class SServ
    {
        string ip;
        int port;
        TcpListener server;
        public List<SSession> sessions;

        public SServ(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            sessions = new List<SSession>();
            Init();
        }

        public void Init()
        {
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            Console.WriteLine("Server has started on {0}:{1}, Waiting for a connections...", ip, port);

            Thread connManager = new Thread(new ThreadStart(GetNewConnections));
            connManager.Start();

        }

        public void GetNewConnections()
        {
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("A client connected.");

                lock (sessions)
                {
                    sessions.Add(new SSession(client));
                }
            }
        }

        public void SendMsg(User user)
        {
            try
            {
                lock (sessions)
                {
                    foreach (SSession session in this.sessions.Where(x => x.Login == user.login))
                    {
                        session.Send(user.message);
                    }
                }
            }
            finally
            {

            }
        }

    }
}