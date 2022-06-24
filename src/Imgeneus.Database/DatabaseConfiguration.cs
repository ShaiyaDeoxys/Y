namespace Imgeneus.Database
{
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the database host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the database port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the database connection username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the database connection password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// SSL connection mode.
        /// </summary>
        public string SslMode { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var connectionString = $"server={Host};userid={Username};pwd={Password};port={Port};database={Database};SSL Mode={SslMode}";
            if (SslMode == "Required") // For Azure database.
                connectionString += ";Ssl CA=BaltimoreCyberTrustRoot.crt.pem;TlsVersion=TLS 1.2";
            return connectionString;
        }
    }
}
