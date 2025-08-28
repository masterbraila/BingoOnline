using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BingoGameOnline.Server.Models;

namespace BingoGameOnline.Server.Services
{
    public class UnconfirmedAccountCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnconfirmedAccountCleanupService> _logger;
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(12); // Run twice a day

        public UnconfirmedAccountCleanupService(IServiceProvider serviceProvider, ILogger<UnconfirmedAccountCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupUnconfirmedAccountsAsync();
                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupUnconfirmedAccountsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;
            var threshold = now.AddDays(-5);
            var usersToDelete = dbContext.Users
                .Where(u => !u.EmailConfirmed && u.CreatedOn < threshold)
                .ToList();

            foreach (var user in usersToDelete)
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Deleted unconfirmed user: {user.Email} (ID: {user.Id})");
                }
                else
                {
                    _logger.LogWarning($"Failed to delete unconfirmed user: {user.Email} (ID: {user.Id})");
                }
            }
        }
    }
}
