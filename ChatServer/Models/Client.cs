using ChatShared.Enums;
using ChatShared.Net.IO;
using System.Net.Sockets;

namespace ChatServer.Models
{
    [Serializable]
    public class Client
    {
        private PacketReader _packetReader;
        private Broadcaster _broadcaster;

        public string Username { get; set; }

        public Guid Id { get; set; }

        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client, Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
            ClientSocket = client;
            Id = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"{DateTime.Now}:\n {Id} Client {Username} has connected to the chat.");

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
                            _broadcaster.BroadcastMessage($"{Username}: {message} {DateTime.Now.ToShortTimeString()}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"{DateTime.Now}:\n{Id} {Username} client disconected.\n" + new string('=', 30));
                    _broadcaster.BroadcastDisconnect(Id.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
