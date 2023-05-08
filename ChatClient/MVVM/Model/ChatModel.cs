using System.Collections.Generic;

namespace ChatClient.MVVM.Model
{
    public class ChatModel
    {
        public string Id { get; set; }

        public UserModel User { get; set; }

        public List<MessageModel> Messages { get; set; }
    }
}
