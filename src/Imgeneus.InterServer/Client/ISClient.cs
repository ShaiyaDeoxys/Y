using InterServer.Common;
using InterServer.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace InterServer.Client
{
    public class ISClient : IInterServerClient
    {
        private readonly InterServerConfig _config;
        private readonly ILogger<IInterServerClient> _logger;

        /// <summary>
        /// SignalR connection.
        /// </summary>
        private readonly HubConnection _connection;

        public ISClient(IOptions<InterServerConfig> options, ILogger<IInterServerClient> logger)
        {
            _config = options.Value;
            _logger = logger;

            if (string.IsNullOrEmpty(_config.Endpoint))
            {
                throw new ArgumentException("Interserver communication is not properly configured!");
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(_config.Endpoint)
                .WithAutomaticReconnect()
                .Build();

            _connection.Closed += Connection_Closed;
            _connection.Reconnecting += Connection_Reconnecting;
            _connection.Reconnected += Connection_Reconnected;

            _connection.On<SessionResponse>(nameof(OnAesKeyResponse), OnAesKeyResponse);
        }

        private Task Connection_Reconnecting(Exception arg)
        {
            _logger.LogInformation("Trying to reconnected to the login server. Err: " + arg.Message);
            return Task.CompletedTask;
        }

        private Task Connection_Reconnected(string arg)
        {
            _logger.LogInformation("Connection the login server restored.");
            OnConnected?.Invoke();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async void Connect()
        {
            if (_connection.State == HubConnectionState.Connected)
                return;

            await _connection.StartAsync();

            if (_connection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("Successfully connected to the login server.");
                OnConnected?.Invoke();
            }
            else
            {
                _logger.LogError("Failed to connect to {0}.", _config.Endpoint);
                Connect();
            }
        }

        /// <inheritdoc/>
        public event Action OnConnected;

        private Task Connection_Closed(Exception ex)
        {
            _logger.LogError("Connection the login server lost. Err: " + ex.Message);
            Connect();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends message to login server.
        /// </summary>
        public async Task Send(ISMessage mesage)
        {
            await _connection.SendAsync(ISHub.MessageTypeToMethodName[mesage.Type], mesage.Payload);
        }

        /// <inheritdoc/>
        public event Action<SessionResponse> OnSessionResponse;

        internal void OnAesKeyResponse(SessionResponse response)
        {
            OnSessionResponse?.Invoke(response);
        }
    }
}
