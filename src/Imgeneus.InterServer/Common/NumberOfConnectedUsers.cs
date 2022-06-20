namespace Imgeneus.InterServer.Common
{
    public struct NumberOfConnectedUsers
    {
        public string ServerName { get; set; }

        public ushort NumberOfPlayers { get; set; }

        public NumberOfConnectedUsers(string name, ushort number)
        {
            ServerName = name;
            NumberOfPlayers = number;
        }
    }
}
