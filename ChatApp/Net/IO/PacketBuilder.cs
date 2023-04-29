using System.IO;

namespace ChatClient.Net.IO
{
    public class PacketBuilder
    {
        private MemoryStream _memoryStream;

        public PacketBuilder()
        {
            _memoryStream = new MemoryStream();
        }
    }
}
