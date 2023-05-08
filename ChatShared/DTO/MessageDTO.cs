namespace ChatShared.DTO
{
    public class MessageDTO
    {
        public string Text { get; set; }

        public DateTime TimeCode { get; set; }

        public UserDTO Sender { get; set; }
    }
}
