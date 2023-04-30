using ChatSharedt.Net.IO;
using System.Net;
using System.Net.Sockets;
using ChatShared.Enums;

namespace ChatServer
{
    public class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                BroadcastConnection();
            }
        }

        static void BroadcastConnection()
        {
            foreach (var client in _users)
            {
                foreach (var item in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(PacketType.Connection);
                    broadcastPacket.WriteMessage(item.Username);
                    broadcastPacket.WriteMessage(item.Id.ToString());

                    var broadcastingPacket = broadcastPacket.GetPacketBytes();
                    client.ClientSocket.Client.Send(broadcastingPacket);
                }
            }
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var client in _users)
            {
                var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(PacketType.Message);
                messagePacket.WriteMessage(message);

                var byteMessagePacket = messagePacket.GetPacketBytes();
                client.ClientSocket.Client.Send(byteMessagePacket);
            }
        }

        public static void BroadcastDisconnect(string id)
        {
            var disconnectedUser = _users.FirstOrDefault(x => x.Id.ToString() == id);
            _users.Remove(disconnectedUser);

            foreach(var client in _users)
            {
                var packet = new PacketBuilder();
                packet.WriteOpCode(PacketType.Disconnection);
                packet.WriteMessage(id);

                var bytePacket = packet.GetPacketBytes();
                client.ClientSocket.Client.Send(bytePacket);
            }

            BroadcastMessage($"{disconnectedUser.Username} leave the chat. {DateTime.Now}");
        }
    }
}