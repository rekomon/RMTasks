namespace RMTasks.Models
{
    public class SMTPConfig
    {
        public string host { get; set; }
        public int port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
    }
}
