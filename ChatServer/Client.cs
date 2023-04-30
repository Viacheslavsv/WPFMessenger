using ChatShared.Enums;
using ChatShared.Net.IO;
using System.Net.Sockets;

namespace ChatServer
{
    public class Client
    {
        private PacketReader _packetReader;

        public string Username { get; set; }

        public Guid Id { get; set; }

        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client)
        {
            ClientSocket = client;
            Id = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"{DateTime.Now} Client {Username} has connected to the chat.");

            Task.Run(() => Process());
        }

        private void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = (PacketType)_packetReader.ReadByte();
                    switch (opcode)
                    {
                        case PacketType.Message:
                            var message = _packetReader.ReadMessage();
                            Console.WriteLine($"{DateTime.Now} {Id} {Username}: {message}");
                            Program.BroadcastMessage($"{Username}: {message} {DateTime.Now.ToShortTimeString()}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"{DateTime.Now}:\n{Id} {Username} client disconected.\n" + new string('=', 30));
                    Program.BroadcastDisconnect(Id.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
