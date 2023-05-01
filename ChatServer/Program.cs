using System.Net;
using System.Net.Sockets;
using ChatServer.Models;

namespace ChatServer
{
    public class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static Broadcaster _broadcaster;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _broadcaster = new Broadcaster(_users);
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                var tcpClient = _listener.AcceptTcpClient();
                var client = new Client(tcpClient, _broadcaster);
                var isClientAlreadyExist = _users.Select(x => x.Username).Contains(client.Username);
                if (isClientAlreadyExist)
                {
                    _broadcaster.BroadcastFailedConnection(client);
                    tcpClient.Close();
                }
                else
                {
                    _users.Add(client);
                    _broadcaster.BroadcastConnection(client);
                }
            }
        }

        private static void BroadcastMessages(Client fromClient, Client toClient)
        {

        }
    }
}