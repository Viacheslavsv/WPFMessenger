using AutoMapper;
using ChatServer.Models;
using ChatShared.DTO;
using ChatShared.Enums;
using ChatSharedt.Net.IO;
using System.Text.Json;

namespace ChatServer
{
    public class Broadcaster
    {
        private List<Client> _users = new();
        private List<Chat> _chats = new();

        public Broadcaster(List<Client> users, List<Chat> chats)
        {
            _users = users;
            _chats = chats;
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
        }

        public void BroadcastFailedConnection(Client failedClient)
        {
            var packet = new PacketBuilder();
            packet.WritePacketType(PacketType.FailedConnection);
            packet.WriteMessage("User with this name is already exists.");

            var bytePacket = packet.GetPacketBytes();
            failedClient.ClientSocket.Client.Send(bytePacket);
        }

        public void BroadcastMessageToAll(string message)
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

        public void BroadcastMessageToUser(Guid fromClientId, Guid toClientId, string message)
        {
            var sender = _users.FirstOrDefault(u => u.Id == fromClientId);
            var recipient = _users.FirstOrDefault(u => u.Id == toClientId);
            var chat = _chats.FirstOrDefault(c=>c.Users.Select(cl=>cl.Id).Contains(fromClientId) && c.Users.Select(cl => cl.Id).Contains(toClientId));
            if(chat is null)
            {
                chat = new Chat()
                {
                    Users = new List<Client>
                    {
                        sender,
                        recipient
                    },
                    Messages = new(),
                    Id = Guid.NewGuid()
                };
            }

            var newMessage = new Message()
            {
                Client = sender,
                Text = message,
                TimeCode = DateTime.Now
            };

            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Message);
            messagePacket.WriteMessage(chat.Id.ToString());
            messagePacket.WriteMessage(newMessage.Text);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            recipient?.ClientSocket.Client.Send(byteMessagePacket);

            chat.Messages.Add(newMessage);
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
                BroadcastUsersList(disconnectedUser);
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
                    Id = user.Id,
                    Username = user.Username
                });
            }
            var usersJson = JsonSerializer.Serialize(usersDto);

            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Users);
            messagePacket.WriteMessage(usersJson);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            client.ClientSocket.Client.Send(byteMessagePacket);
        }

        public void BroadcastChat(Guid clientSenderId, Guid targetClientId)
        {
            var sender = _users.FirstOrDefault(u => u.Id == clientSenderId);

            var chat = _chats.FirstOrDefault(c => c.Users.Select(cl => cl.Id).Contains(clientSenderId) && c.Users.Select(cl => cl.Id).Contains(targetClientId));
            if (chat is null)
            {
                var recipient = _users.FirstOrDefault(u => u.Id == targetClientId);
                chat = new Chat()
                {
                    Users = new List<Client>
                    {
                        sender,
                        recipient
                    },
                    Messages = new(),
                    Id = Guid.NewGuid()
                };
                _chats.Add(chat);
            }

            var mapperConfig = new MapperConfiguration(c =>
            {
                c.CreateMap<Message, MessageDTO>();
                c.CreateMap<Client, UserDTO>();
                c.CreateMap<Chat, ChatDTO>();
            });
            var mapper = new Mapper(mapperConfig);

            var chatsDto = mapper.Map<ChatDTO>(chat);

            var chatsJson = JsonSerializer.Serialize(chatsDto);

            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Chat);
            messagePacket.WriteMessage(chatsJson);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            sender.ClientSocket.Client.Send(byteMessagePacket);
        }
    }
}
