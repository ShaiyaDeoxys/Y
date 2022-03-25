namespace Imgeneus.Core.Structures.Configuration
{
    public sealed class WorldConfiguration
    {
        /// <summary>
        /// Gets or sets the world's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client build version
        /// </summary>
        public int BuildVersion { get; set; }

        /// <summary>
        /// Public ip address.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Max number os connection.
        /// </summary>
        public ushort MaximumNumberOfConnections { get; set; }
    }
}
