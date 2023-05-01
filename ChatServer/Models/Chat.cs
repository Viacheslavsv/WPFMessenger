namespace ChatServer.Models
{
    public class Chat
    {
        public Guid Id { get; set; }

        public List<Client> Clients { get; set; }

        public List<Message> Messages { get; set; }
    }
}
