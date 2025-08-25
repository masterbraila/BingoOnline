using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using BingoGameOnline.Client.Services;

namespace BingoGameOnline.Client.Services
{
    /// <summary>
    /// Service for managing admin-side SignalR connection and game state in the AdminTickets page.
    /// </summary>
    public class AdminBingoHubService
    {
        // --- SignalR Hub Connection ---
        private readonly NavigationManager _navigationManager;
        public HubConnection? HubConnection { get; private set; }

        // --- State ---
        public List<UserInfo> Users { get; private set; } = new();
        public List<int> DrawnNumbers { get; private set; } = new();
        public string? LineWinAnnouncement { get; private set; }
        public string? BingoWinAnnouncement { get; private set; }
        public string? ConfirmationMessage { get; private set; }
        public BingoTicket? LastTicket { get; private set; }

        // --- Events ---
        public event Action? OnStateChanged;
        public event Action? OnUsersChanged;

        public AdminBingoHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        /// <summary>
        /// Connects to the SignalR hub and registers all admin event handlers.
        /// </summary>
        public async Task ConnectAsync()
        {
            var hubUrl = _navigationManager.ToAbsoluteUri("/bingoHub").ToString();
            HubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();
            RegisterHandlers();
            await HubConnection.StartAsync();
            await LoadUsers();
        }

        /// <summary>
        /// Registers SignalR event handlers for admin events.
        /// </summary>
        private void RegisterHandlers()
        {
            if (HubConnection == null) return;
            HubConnection.On<int[]>("CalledNumbersSync", (numbers) =>
            {
                DrawnNumbers = new List<int>(numbers);
                OnStateChanged?.Invoke();
            });
            HubConnection.On<int, int, string>("LineWinAnnounced", (grid, rowInGrid, winnerName) =>
            {
                LineWinAnnouncement = $"{winnerName} has won a line!";
                OnStateChanged?.Invoke();
            });
            HubConnection.On<string>("BingoWinAnnounced", (winnerName) =>
            {
                BingoWinAnnouncement = $"{winnerName} has won BINGO!";
                OnStateChanged?.Invoke();
            });
            HubConnection.On<BingoTicket>("ReceiveTicket", (ticket) =>
            {
                LastTicket = ticket;
                OnStateChanged?.Invoke();
            });
            HubConnection.On<string>("TicketGeneratedAndSent", (message) =>
            {
                ConfirmationMessage = message;
                OnStateChanged?.Invoke();
            });
            HubConnection.On<List<UserInfo>>("UserListUpdated", (updatedUsers) =>
            {
                Users = updatedUsers ?? new List<UserInfo>();
                OnUsersChanged?.Invoke();
            });
            HubConnection.On<int>("NumberCalled", (number) =>
            {
                if (!DrawnNumbers.Contains(number))
                    DrawnNumbers.Add(number);
                OnStateChanged?.Invoke();
            });
            HubConnection.On("CalledNumbersReset", () =>
            {
                DrawnNumbers.Clear();
                OnStateChanged?.Invoke();
            });
            HubConnection.On("NewGameStarted", () =>
            {
                DrawnNumbers.Clear();
                LastTicket = null;
                ConfirmationMessage = null;
                LineWinAnnouncement = null;
                BingoWinAnnouncement = null;
                OnStateChanged?.Invoke();
            });
        }

        /// <summary>
        /// Loads the list of connected users from the server.
        /// </summary>
        public async Task LoadUsers()
        {
            if (HubConnection != null)
            {
                var result = await HubConnection.InvokeAsync<List<UserInfo>>("GetConnectedUsers");
                Users = result ?? new List<UserInfo>();
                OnUsersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Calls a number in the game.
        /// </summary>
        public async Task CallNumber()
        {
            if (HubConnection != null)
                await HubConnection.InvokeAsync("CallNumber");
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public async Task NewGame()
        {
            if (HubConnection != null)
                await HubConnection.InvokeAsync("NewGame");
        }

        /// <summary>
        /// Generates and sends a ticket to a user.
        /// </summary>
        public async Task GenerateAndSendTicket(string connectionId)
        {
            if (HubConnection != null && !string.IsNullOrWhiteSpace(connectionId))
            {
                ConfirmationMessage = null;
                await HubConnection.InvokeAsync("GenerateAndSendTicket", connectionId);
            }
        }

        /// <summary>
        /// Gets a user's ticket from the server.
        /// </summary>
        public async Task<BingoTicket?> GetUserTicket(string connectionId)
        {
            if (HubConnection != null && !string.IsNullOrWhiteSpace(connectionId))
            {
                try
                {
                    return await HubConnection.InvokeAsync<BingoTicket>("GetUserTicket", connectionId);
                }
                catch { return null; }
            }
            return null;
        }
    }

}
