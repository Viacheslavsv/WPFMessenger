namespace ChatDAL.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public ICollection<MessageEntity> Messages { get; set; }

        public ICollection<ChatEntity> Chats { get; set; }
    }
}
