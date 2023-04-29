using ChatClient.MVVM.Core;
using ChatClient.Net;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel
    {
        private Server _server;

        public RelayCommand ConnectToServerCommand { get; set; }

        public MainViewModel()
        {
            _server = new Server();
            ConnectToServerCommand = new(o => _server.ConnectToServer());
        }
    }
}
