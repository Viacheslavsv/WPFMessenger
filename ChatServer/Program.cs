using System.Net;
using System.Net.Sockets;
using ChatServer.Models;

namespace ChatServer
{
    public class Program
    {
        static List<Client> _users;
        static List<Chat> _chats;
        static TcpListener _listener;
        static Broadcaster _broadcaster;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _chats = new List<Chat>();
            _broadcaster = new Broadcaster(_users, _chats);
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                var tcpClient = _listener.AcceptTcpClient();
                var client = new Client(tcpClient, _broadcaster, _users);
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
    }
}