namespace ChatServer.Models
{
    public class Message
    {
        public string Text { get; set; }

        public DateTime TimeCode { get; set; }

        public Client Client{ get; set; }
    }
}
