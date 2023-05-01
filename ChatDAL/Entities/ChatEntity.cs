namespace ChatDAL.Entities
{
    public class ChatEntity
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public ICollection<UserEntity> Users { get; set; }

        public ICollection<MessageEntity> Messages { get; set; }
    }
}