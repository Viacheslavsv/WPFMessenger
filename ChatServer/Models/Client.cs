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
                            var recipientIdString = _packetReader.ReadMessage();
                            var recipientId = Guid.Parse(recipientIdString);
                            var recipientUsername = _clients.FirstOrDefault(c => c.Id == recipientId)?.Username;

                            var message = _packetReader.ReadMessage();
                            var broadcastingMessage = $"{DateTime.Now.ToString("HH:mm")} {Username}: {message}";

                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} From {Id} {Username} To {recipientId}: {message}");
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
                    Console.WriteLine($"{DateTime.Now}:\n{Id} {Username} client disconected.\n" + new string('=', 30));
                    _broadcaster.BroadcastDisconnect(Id.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
