namespace ChatDAL.Entities
{
    public class MessageEntity
    {
        public Guid Id { get; set; }

        public string Text { get; set; }

        public DateTime TimeCode { get; set; }

        public Guid UserId { get; set; }
        public UserEntity User { get; set; }

        public Guid ChatId { get; set; }
        public ChatEntity Chat { get; set; }
    }
}