namespace Imgeneus.InterServer.Common
{
    public struct WorldStateChanged
    {
        public string ServerName { get; set; }

        public bool IsRunning { get; set; }

        public WorldStateChanged(string name, bool isRunning)
        {
            ServerName = name;
            IsRunning = isRunning;
        }
    }
}
