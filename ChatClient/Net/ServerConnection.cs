using ChatShared.Net.IO;
using ChatSharedt.Net.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatShared.Enums;

namespace ChatClient.Net
{
    public class ServerConnection
    {
        private static TcpClient _tcpClient;

        public PacketReader PacketReader;

        public event Action onConnectedEvent;
        public event Action onMessageReceivedEvent;
        public event Action onDisconnectedEvent;
        public event Action onFailedConnection;
        public event Action onUsersReceived;
        public event Action onChatReceived;

        public void ConnectToServer(string username, string ipAddress)
        {
            _tcpClient = _tcpClient is null ? new TcpClient() : _tcpClient;
            if (!_tcpClient.Connected)
            {
                try
                {
                    _tcpClient.Connect(ipAddress, 7891);
                }
                catch (SocketException ex)
                {
                    throw ex;
                }

                PacketReader = new PacketReader(_tcpClient.GetStream());

                if (!string.IsNullOrEmpty(username))
                {
                    var bytePacket = CreateBytePacket(username);

                    _tcpClient.Client.Send(bytePacket);
                }

                ReadPackets();
            }
        }

        public void DisconnectFromServer()
        {
            if (_tcpClient.Connected)
            {
                _tcpClient.GetStream().Close();
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        private byte[] CreateBytePacket(string packet)
        {
            var connectPacket = new PacketBuilder();
            connectPacket.WritePacketType(0);
            connectPacket.WriteMessage(packet);

            return connectPacket.GetPacketBytes();
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var opcode = (PacketType)PacketReader.ReadByte();
                        switch (opcode)
                        {
                            case PacketType.Connection:
                                onConnectedEvent?.Invoke();
                                break;
                            case PacketType.Message:
                                onMessageReceivedEvent?.Invoke();
                                break;
                            case PacketType.Disconnection:
                                onDisconnectedEvent?.Invoke();
                                break;
                            case PacketType.FailedConnection:
                                onFailedConnection?.Invoke();
                                break;
                            case PacketType.Users:
                                onUsersReceived?.Invoke();
                                break;
                            case PacketType.Chat:
                                onChatReceived?.Invoke();
                                break;
                            default:
                                Console.WriteLine("Unknown opcode");
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        DisconnectFromServer();
                        break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Message);
            messagePacket.WriteMessage(message);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            _tcpClient.Client.Send(byteMessagePacket);
        }

        public void SendMessageToUser(Guid userId, string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Message);
            messagePacket.WriteMessage(userId.ToString());
            messagePacket.WriteMessage(message);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            _tcpClient.Client.Send(byteMessagePacket);
        }

        public void RequestForChat(Guid userId)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WritePacketType(PacketType.Chat);
            messagePacket.WriteMessage(userId.ToString());

            var byteMessagePacket = messagePacket.GetPacketBytes();
            _tcpClient.Client.Send(byteMessagePacket);
        }
    }
}
