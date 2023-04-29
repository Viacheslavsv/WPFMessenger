using System.Net.Sockets;

namespace ChatClient.Net
{
    public class Server
    {
        TcpClient _tcpClient;
        public Server()
        {
            _tcpClient = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!_tcpClient.Connected)
            {
                string hostName = "127.0.0.1";
                _tcpClient.Connect(hostName, 7891);
            }
        }
    }
}
