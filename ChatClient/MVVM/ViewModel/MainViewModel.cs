using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel
    {
        private ServerConnection _server;

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        public string Username { get; set; }
        public string Message { get; set; }

        public MainViewModel()
        {
            Users = new();
            Messages = new();

            _server = new ServerConnection();
            _server.connectedEvent += UserConnected;
            _server.messageReceivedEvent += MessageReceived;
            _server.disconnectedEvent += UserDisconnected;

            ConnectToServerCommand = new(o => _server.ConnectToServer(Username),
                o => !string.IsNullOrEmpty(Username));

            SendMessageCommand = new(o => _server.SendMessageToServer(Message),
                o => !string.IsNullOrEmpty(Message));
        }

        private void UserDisconnected()
        {
            var id = _server.PacketReader.ReadMessage();
            var user = Users.FirstOrDefault(u => u.Id == id);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var message = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private void UserConnected()
        {
            var user = new UserModel()
            {
                Username = _server.PacketReader.ReadMessage(),
                Id = _server.PacketReader.ReadMessage()
            };

            if (!Users.Any(u => u.Id == user.Id))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
    }
}
