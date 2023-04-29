using System.Net.Sockets;

namespace ChatServer
{
    public class Client
    {
        public string Username { get; set; }

        public Guid Id { get; set; }

        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client)
        {
            ClientSocket = client;
            Id = Guid.NewGuid();

            Console.WriteLine($"{DateTime.Now} Client {Username} has connected to the chat.");
        }
    }
}
