using System.Net.Sockets;
using System.Text;

namespace ChatShared.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private NetworkStream _networkStream;

        public PacketReader(NetworkStream networkStream) : base(networkStream)
        {
            _networkStream = networkStream;
        }

        public string ReadMessage()
        {
            var length = ReadInt32();
            var messageBuffer = new byte[length];

            _networkStream.Read(messageBuffer, 0, length);
            var message = Encoding.ASCII.GetString(messageBuffer);

            return message;
        }
    }
}
