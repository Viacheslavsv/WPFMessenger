using ChatShared.Enums;
using System.Text;

namespace ChatSharedt.Net.IO
{
    public class PacketBuilder
    {
        private MemoryStream _memoryStream;

        public PacketBuilder()
        {
            _memoryStream = new MemoryStream();
        }

        public void WritePacketType(PacketType opcode)
        {
            _memoryStream.WriteByte((byte)opcode);
        }

        public void WriteMessage(string message)
        {
            var messageLength = message.Length;
            var convertedMessage = BitConverter.GetBytes(messageLength);
            var encodedMessage = Encoding.ASCII.GetBytes(message);

            _memoryStream.Write(convertedMessage);
            _memoryStream.Write(encodedMessage);
        }

        public byte[] GetPacketBytes() 
        {
            return _memoryStream.ToArray();
        }
    }
}
