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
        private TcpClient _tcpClient;

        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action messageReceivedEvent;
        public event Action disconnectedEvent;

        public ServerConnection()
        {
            _tcpClient = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!_tcpClient.Connected)
            {
                string hostName = "127.0.0.1";
                _tcpClient.Connect(hostName, 7891);

                PacketReader = new PacketReader(_tcpClient.GetStream());

                if (!string.IsNullOrEmpty(username)) 
                {
                    var bytePacket = CreateBytePacket(username);

                    _tcpClient.Client.Send(bytePacket);
                }

                ReadPackets();
            }
        }

        private byte[] CreateBytePacket(string packet)
        {
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOpCode(0);
            connectPacket.WriteMessage(packet);

            return connectPacket.GetPacketBytes();
        }


        private void ReadPackets()
        {
            Task.Run(() => 
            {
                while (true) 
                {
                    var opcode = (PacketType)PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case PacketType.Connection:
                            connectedEvent?.Invoke();
                            break;
                        case PacketType.Message:
                            messageReceivedEvent?.Invoke();
                            break;
                        case PacketType.Disconnection:
                            disconnectedEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("Unknown opcode");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(PacketType.Message);
            messagePacket.WriteMessage(message);

            var byteMessagePacket = messagePacket.GetPacketBytes();
            _tcpClient.Client.Send(byteMessagePacket);
        }
    }
}
