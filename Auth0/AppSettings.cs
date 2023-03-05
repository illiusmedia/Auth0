namespace API
{
    public class AppSettings
    {
        public ClientAPI ClientAPI { get; set; }
    }

    public class ClientAPI
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Audience { get; set; }
    }
}
