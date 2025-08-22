using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using BingoGameOnline.Server.Models;

namespace BingoGameOnline.Server.Hubs
{
    public class BingoHub : Hub
    {
        private static Dictionary<string, string> ConnectedUsers = new(); // connectionId -> playerName
        private static HashSet<int> CalledNumbers = new();
        // Store each user's current ticket by connectionId
        private static Dictionary<string, BingoTicket> UserTickets = new();
        private static Random rng = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await BroadcastUserList();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUsers.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
            await BroadcastUserList();
        }

        // Called when a player joins a game room
        public async Task JoinGame(string room, string playerName)
        {
            ConnectedUsers[Context.ConnectionId] = playerName;
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("PlayerJoined", playerName);
            await BroadcastUserList();
        }

        // Called when a player sends a bingo number
        public async Task SendNumber(string room, string playerName, int number)
        {
            await Clients.Group(room).SendAsync("ReceiveNumber", playerName, number);
        }

        // Called when a player calls Bingo
        public async Task CallBingo(string room, string playerName)
        {
            await Clients.Group(room).SendAsync("BingoCalled", playerName);
        }

        // Admin generates a ticket for a player
        public async Task GenerateAndSendTicket(string playerConnectionId)
        {
            BingoTicket ticket = null!;
            int maxTries = 10;
            bool success = false;
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    ticket = GenerateBingoTicket();
                    success = true;
                    break;
                }
                catch { /* try again */ }
            }
            if (!success)
            {
                await Clients.Caller.SendAsync("TicketGenerationFailed", "Failed to generate a valid ticket after several attempts.");
                return;
            }
                // Store the ticket for the user
                UserTickets[playerConnectionId] = ticket;
                await Clients.Client(playerConnectionId).SendAsync("ReceiveTicket", ticket);
            string userName = ConnectedUsers.ContainsKey(playerConnectionId) ? ConnectedUsers[playerConnectionId] : playerConnectionId;
            await Clients.Caller.SendAsync("TicketGeneratedAndSent", $"Ticket generated successful for user {userName}");
        }
        // Admin: Get a user's current ticket (numbers only, not marks)
        public Task<BingoTicket?> GetUserTicket(string connectionId)
        {
            UserTickets.TryGetValue(connectionId, out var ticket);
            return Task.FromResult(ticket);
        }

        // Admin: Get list of connected users
        public Task<List<UserInfo>> GetConnectedUsers()
        {
            var list = new List<UserInfo>();
            UserTickets.Remove(Context.ConnectionId);
            foreach (var kvp in ConnectedUsers)
                list.Add(new UserInfo { ConnectionId = kvp.Key, PlayerName = kvp.Value });
            return Task.FromResult(list);
        }

        // Admin calls a number
        public async Task CallNumber()
        {
            // Pick a unique random number from 1 to 90
            var available = new List<int>();
            for (int i = 1; i <= 90; i++)
                if (!CalledNumbers.Contains(i))
                    available.Add(i);
            if (available.Count == 0)
            {
                await Clients.Caller.SendAsync("NoNumbersLeft");
                return;
            }
            int number = available[rng.Next(available.Count)];
            CalledNumbers.Add(number);
            await Clients.All.SendAsync("NumberCalled", number);
        }

        // Admin: Reset called numbers
        public async Task ResetCalledNumbers()
        {
            CalledNumbers.Clear();
            await Clients.All.SendAsync("CalledNumbersReset");
        }

        // Called when a user claims a line win
        public async Task LineWin(int grid, int rowInGrid, string playerName)
        {
            await Clients.All.SendAsync("LineWinAnnounced", grid, rowInGrid, playerName);
        }

        // Called when a user claims a full house win
        public async Task FullHouseWin(int grid, string playerName)
        {
            await Clients.All.SendAsync("FullHouseWinAnnounced", grid, playerName);
        }

        // Admin: Start a new game
        public async Task NewGame()
        {
            CalledNumbers.Clear();
            await Clients.All.SendAsync("NewGameStarted");
        }

        // Client requests to disconnect and remove from user list
        public async Task DisconnectPlayer()
        {
            ConnectedUsers.Remove(Context.ConnectionId);
            await BroadcastUserList();
        }

        // Helper: Generate a random Bingo ticket
        private BingoTicket GenerateBingoTicket()
        {
            // 5 tickets, each 3x9
            var ticket = new BingoTicket();
            var rand = new Random();
            int tickets = 5;
            int rowsPerTicket = 3;
            int cols = 9;
            int totalRows = tickets * rowsPerTicket;
            int[] colMin = { 1, 10, 20, 30, 40, 50, 60, 70, 80 };
            int[] colMax = { 9, 19, 29, 39, 49, 59, 69, 79, 90 };
            int[] colCounts = { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
            // All state must be reset for each generation
            bool[][] mask = new bool[totalRows][];
            for (int r = 0; r < totalRows; r++)
                mask[r] = new bool[cols];
            int[] rowCounts = new int[totalRows];
            int[] colTotals = new int[cols];
            bool FillMask(int row, int col)
            {
                if (row == totalRows) return true;
                int nextRow = (col == cols - 1) ? row + 1 : row;
                int nextCol = (col + 1) % cols;
                // Try placing true if constraints allow
                bool[] options = new bool[] { true, false };
                options = options.OrderBy(_ => rand.Next()).ToArray();
                foreach (var val in options)
                {
                    if (val)
                    {
                        if (rowCounts[row] >= 5 || colTotals[col] >= colCounts[col]) continue;
                        mask[row][col] = true;
                        rowCounts[row]++;
                        colTotals[col]++;
                    }
                    else
                    {
                        mask[row][col] = false;
                    }
                    if (col == cols - 1 && rowCounts[row] != 5)
                    {
                        if (val)
                        {
                            rowCounts[row]--;
                            colTotals[col]--;
                        }
                        continue;
                    }
                    if (FillMask(nextRow, nextCol)) return true;
                    if (val)
                    {
                        rowCounts[row]--;
                        colTotals[col]--;
                    }
                }
                return false;
            }
            bool success = FillMask(0, 0);
            if (!success) throw new Exception("Failed to generate a valid Bingo mask.");
            // Step 2: Prepare numbers for each column
            List<int>[] colNumbers = new List<int>[cols];
            for (int c = 0; c < cols; c++)
            {
                colNumbers[c] = new List<int>();
                for (int n = colMin[c]; n <= colMax[c]; n++)
                    colNumbers[c].Add(n);
                colNumbers[c] = colNumbers[c].OrderBy(_ => rand.Next()).ToList();
            }
            // Step 3: Fill the ticket
            int?[][] grid = new int?[totalRows][];
            for (int r = 0; r < totalRows; r++)
            {
                grid[r] = new int?[cols];
                for (int c = 0; c < cols; c++)
                {
                    if (mask[r][c])
                    {
                        grid[r][c] = colNumbers[c][0];
                        colNumbers[c].RemoveAt(0);
                    }
                    else
                    {
                        grid[r][c] = null;
                    }
                }
            }
            // Assign to ticket
            for (int r = 0; r < totalRows; r++)
                ticket.Numbers[r] = grid[r];
            return ticket;
        }

        // Helper: Broadcast user list to all clients
        private async Task BroadcastUserList()
        {
            var list = new List<UserInfo>();
            foreach (var kvp in ConnectedUsers)
                list.Add(new UserInfo { ConnectionId = kvp.Key, PlayerName = kvp.Value });
            await Clients.All.SendAsync("UserListUpdated", list);
        }
    }
}
