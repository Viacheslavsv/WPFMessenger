using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatShared.DTO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
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
        public RelayCommand DisconnectFromServerCommand { get; set; }

        public string Username { get; set; }
        public string IpAddress { get; set; } = "127.0.0.1";
        public string Message { get; set; }

        public MainViewModel()
        {
            Users = new();
            Messages = new();

            ConnectToServerCommand = new(o =>
            {
                InitializeServer();
                try
                {
                    _server.ConnectToServer(Username, IpAddress);
                }
                catch (SocketException)
                {
                    Application.Current.Dispatcher.Invoke(() => Messages.Add("Problems with connection to the server. May be incorrect ip address."));
                }
            },
                o => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(IpAddress));

            SendMessageCommand = new(o =>
            {
                _server.SendMessageToServer(Message);
            },
                o => !string.IsNullOrEmpty(Message));

            DisconnectFromServerCommand = new(o =>
            {
                _server.DisconnectFromServer();
                Users.Clear();
                Messages.Clear();
            },
                s => _server is not null);
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

        private void ChatReceived()
        {
            var chatsJson = _server.PacketReader.ReadMessage();
            var chats = JsonSerializer.Deserialize<List<UserDTO>>(chatsJson);

            Application.Current.Dispatcher.Invoke(() => Users.Clear());
            foreach (var user in chats)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(new UserModel()
                {
                    Id = user.Id,
                    Username = user.Username
                }));
            }
        }

        private void InitializeServer()
        {
            _server = new ServerConnection();
            _server.onConnectedEvent += UserConnected;
            _server.onMessageReceivedEvent += MessageReceived;
            _server.onDisconnectedEvent += UserDisconnected;
            _server.onFailedConnection += MessageReceived;
            _server.onChatsReceived += ChatReceived;
        }
    }
}
