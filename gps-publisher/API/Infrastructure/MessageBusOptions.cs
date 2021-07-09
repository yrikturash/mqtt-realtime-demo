namespace API.Infrastructure
{
    public class MessageBusOptions
    {
        public string HostName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
    }
}
