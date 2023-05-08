using AutoMapper;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatShared.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel
    {
        private ServerConnection _server;
        private bool _isConnected = false;

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public ObservableCollection<ChatModel> Chats { get; set; }

        public ICommand ItemSelectedCommand { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageToServerCommand { get; set; }
        public RelayCommand SendMessageToUserCommand { get; set; }
        public RelayCommand DisconnectFromServerCommand { get; set; }

        public string Username { get; set; }
        public string IpAddress { get; set; } = "127.0.0.1";
        public string Message { get; set; }
        public ChatModel CurrentChat { get; set; }
        public UserModel CurrentUser { get; set; }

        public MainViewModel()
        {
            CurrentChat = new();
            Users = new();
            Messages = new();
            Chats = new();

            ConnectToServerCommand = new(o =>
            {
                InitializeServer();
                try
                {
                    _server.ConnectToServer(Username, IpAddress);
                    _isConnected = true;
                }
                catch (SocketException)
                {
                    Application.Current.Dispatcher.Invoke(() => Messages.Add("Problems with connection to the server. May be incorrect ip address."));
                }
            },
                o => !_isConnected && Username?.Length > 3 && !string.IsNullOrEmpty(IpAddress));

            ItemSelectedCommand = new RelayCommand(ExecuteItemSelectedCommand);

            SendMessageToServerCommand = new(o =>
            {
                _server.SendMessageToServer(Message);
            },
                o => !string.IsNullOrEmpty(Message));

            SendMessageToUserCommand = new(o =>
            {
                var message = new MessageModel()
                {
                    Sender = null,
                    Text = Message,
                    TimeCode = DateTime.Now
                };

                Application.Current.Dispatcher.Invoke(() => Messages.Add($"{DateTime.Now.ToString("HH:mm")}: " + message.ToString()));

                _server.SendMessageToUser(Guid.Parse(CurrentUser.Id), Message);
            },
             o => !string.IsNullOrEmpty(Message) && CurrentUser is not null);

            DisconnectFromServerCommand = new(o =>
            {
                _server.DisconnectFromServer();
                Users.Clear();
                Messages.Clear();
                _isConnected = false;
            },
                s => _isConnected && _server is not null);
        }

        private void ExecuteItemSelectedCommand(object selectedUser)
        {
            var selected = selectedUser as UserModel;
            if (selected is not null)
            {

                CurrentUser = Users.FirstOrDefault(x => x.Username == selected.Username);
                _server.RequestForChat(Guid.Parse(CurrentUser.Id));
            }
        }

        private void UserDisconnected()
        {
            var id = _server.PacketReader.ReadMessage();
            var user = Users.FirstOrDefault(u => u.Id == id);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var chatId = _server.PacketReader.ReadMessage();
            var message = _server.PacketReader.ReadMessage();
            if (chatId == CurrentChat.Id)
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
            }
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

        private void UsersListReceived()
        {
            var usersJson = _server.PacketReader.ReadMessage();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(usersJson);

            Application.Current.Dispatcher.Invoke(() => Users.Clear());
            foreach (var user in users)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(new UserModel()
                {
                    Id = user.Id.ToString(),
                    Username = user.Username
                }));
            }
        }

        private void ChatRecieved()
        {
            var chatsJson = _server.PacketReader.ReadMessage();
            var chatDto = JsonSerializer.Deserialize<ChatDTO>(chatsJson);

            var mapperConfig = new MapperConfiguration(c =>
            {
                c.CreateMap<MessageDTO, MessageModel>();
                c.CreateMap<UserDTO, UserModel>();
                c.CreateMap<ChatDTO, ChatModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var chat = mapper.Map<ChatModel>(chatDto);

            var userDto = chatDto.Users.FirstOrDefault(u => u.Username != Username);

            chat.User = mapper.Map<UserModel>(userDto);

            Application.Current.Dispatcher.Invoke(() => Messages.Clear());

            CurrentChat.Id = chat.Id;
            CurrentChat.User = chat.User;
            CurrentChat.Messages = chat.Messages;

            Application.Current.Dispatcher.Invoke(() => Messages.Clear());

            var messages = CurrentChat.Messages.Select(x => x.ToString().Replace(Username, ""));
            foreach (var message in messages)
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
            }
        }

        private void InitializeServer()
        {
            _server = new ServerConnection();
            _server.onConnectedEvent += UserConnected;
            _server.onMessageReceivedEvent += MessageReceived;
            _server.onDisconnectedEvent += UserDisconnected;
            _server.onFailedConnection += MessageReceived;
            _server.onUsersReceived += UsersListReceived;
            _server.onChatReceived += ChatRecieved;
        }
    }
}
