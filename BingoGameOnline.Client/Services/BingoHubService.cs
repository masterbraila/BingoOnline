using Microsoft.AspNetCore.SignalR.Client;

using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace BingoGameOnline.Client.Services
{
        public class BingoHubService
        {
            private readonly NavigationManager _navigationManager;
            public HubConnection? HubConnection { get; private set; }
            public string PlayerName { get; private set; } = string.Empty;
            public string Room { get; private set; } = "default";
            public event Action<BingoTicket?>? OnTicketReceived;
            public event Action? OnNewGame;
            public event Action? OnDisconnected;
            public event Action<int>? OnNumberCalled;

            // Helper to clear all event handlers (for safe re-subscription)
            public void ClearEventHandlers()
            {
                OnTicketReceived = null;
                OnNewGame = null;
                OnDisconnected = null;
                OnNumberCalled = null;
            }

        public BingoHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task ConnectAsync(string playerName)
        {
            if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
                return;
            PlayerName = playerName;
            // Use the correct server port for SignalR hub connection in development
            var hubUrl = _navigationManager.ToAbsoluteUri("/bingoHub").ToString();
            HubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();
            RegisterHandlers();
            await HubConnection.StartAsync();
            await HubConnection.InvokeAsync("JoinGame", Room, PlayerName);
        }

        private void RegisterHandlers()
        {
            if (HubConnection == null) return;
            HubConnection.On<BingoTicket>("ReceiveTicket", (ticket) =>
            {
                OnTicketReceived?.Invoke(ticket);
            });
            HubConnection.On("NewGameStarted", () =>
            {
                OnNewGame?.Invoke();
            });
            HubConnection.On<int>("NumberCalled", (number) =>
            {
                OnNumberCalled?.Invoke(number);
            });
        }

        public async Task DisconnectAsync()
        {
            if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
            {
                await HubConnection.InvokeAsync("DisconnectPlayer");
                await HubConnection.StopAsync();
                await HubConnection.DisposeAsync();
                HubConnection = null;
                OnDisconnected?.Invoke();
            }
        }
    }

    public class BingoTicket
    {
        public int?[][] Numbers { get; set; } = new int?[15][];
    }
}
