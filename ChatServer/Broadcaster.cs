using ChatServer.Models;
using ChatShared.DTO;
using ChatShared.Enums;
using ChatSharedt.Net.IO;
using System.Text.Json;

namespace ChatServer
{
    public class Broadcaster
    {
        private  List<Client> _users =new();

        public Broadcaster(List<Client> users)
        {
            _users = users;
        }

        public void BroadcastConnection(Client item)
        {
            foreach (var client in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WritePacketType(PacketType.Connection);
                broadcastPacket.WriteMessage(item.Username);
                broadcastPacket.WriteMessage(item.Id.ToString());

                var broadcastingPacket = broadcastPacket.GetPacketBytes();
                client.ClientSocket.Client.Send(broadcastingPacket);
            }

            BroadcastUsersList(item);
            BroadcastMessage($"{item.Username} connected to the chat. {DateTime.Now.ToShortTimeString()}");
        }

        public void BroadcastFailedConnection(Client failedClient)
        {
            var packet = new PacketBuilder();
            packet.WritePacketType(PacketType.FailedConnection);
            packet.WriteMessage("User with this name is already exists.");

            var bytePacket = packet.GetPacketBytes();
            failedClient.ClientSocket.Client.Send(bytePacket);

        }

        public void BroadcastMessage(string message)
        {
            foreach (var client in _users)
            {
                var messagePacket = new PacketBuilder();
                messagePacket.WritePacketType(PacketType.Message);
                messagePacket.WriteMessage(message);

                var byteMessagePacket = messagePacket.GetPacketBytes();
                client.ClientSocket.Client.Send(byteMessagePacket);
            }
        }

        public void BroadcastDisconnect(string id)
        {
            var disconnectedUser = _users.FirstOrDefault(x => x.Id.ToString() == id);
            if (disconnectedUser is not null)
            {
                _users.Remove(disconnectedUser);

                foreach (var client in _users)
                {
                    var packet = new PacketBuilder();
                    packet.WritePacketType(PacketType.Disconnection);
                    packet.WriteMessage(id ?? string.Empty);

                    var bytePacket = packet.GetPacketBytes();
                    client.ClientSocket.Client.Send(bytePacket);
                }

                BroadcastMessage($"{disconnectedUser.Username} leave the chat. {DateTime.Now.ToShortTimeString()}");
            }
        }

        public void BroadcastUsersList(Client client)
        {
            var usersDto = new List<UserDTO>();
            foreach (var user in _users)
            {
                if (user.Id == client.Id)
                {
                    continue;
                }

                usersDto.Add(new UserDTO()
                {
                    Id = user.Id.ToString(),
                    Username = user.Username
                });
            }
            var usersJson = JsonSerializer.Serialize(usersDto);

            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Chats);
            messagePacket.WriteMessage(usersJson);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            client.ClientSocket.Client.Send(byteMessagePacket);
        }
    }
}
