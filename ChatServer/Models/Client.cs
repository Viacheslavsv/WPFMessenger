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

        private List<Client> _clients;

        public string Username { get; set; }

        public Guid Id { get; set; }

        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client, Broadcaster broadcaster, List<Client> clients)
        {
            _clients = clients;
            _broadcaster = broadcaster;
            ClientSocket = client;
            Id = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"{DateTime.Now}: Client {Username} with id {Id} has connected to the chat.");
            Console.ResetColor();

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
                            var recipientIdString = _packetReader.ReadMessage();
                            var recipientId = Guid.Parse(recipientIdString);
                            var recipientUsername = _clients.FirstOrDefault(c => c.Id == recipientId)?.Username;

                            var message = _packetReader.ReadMessage();
                            var broadcastingMessage = $"{DateTime.Now.ToString("HH:mm")} {Username}: {message}";

                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine($"{DateTime.Now} From {Id} To {recipientId}: {message}");
                            Console.ResetColor();

                            _broadcaster.BroadcastMessageToUser(Id, recipientId, broadcastingMessage);
                            break;
                        case PacketType.Chat:
                            recipientIdString = _packetReader.ReadMessage();
                            recipientId = Guid.Parse(recipientIdString);

                            _broadcaster.BroadcastChat(Id, recipientId);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"{DateTime.Now}: Client {Username} with id {Id} has disconnected to the chat.");
                    Console.ResetColor();

                    _broadcaster.BroadcastDisconnect(Id.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
