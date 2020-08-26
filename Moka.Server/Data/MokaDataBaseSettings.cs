namespace Moka.Server.Data
{
    public class MokaDataBaseSettings : IMokaDataBaseSettings
    {
        public string UsersCollectionName { get; set; }
        public string MessagesCollectionName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }
    public interface IMokaDataBaseSettings
    {
        string UsersCollectionName { get; set; }
        string MessagesCollectionName { get; set; }
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }


    }
}