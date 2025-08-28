using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using BingoGameOnline.Server.Models;

namespace BingoGameOnline.Server.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    public bool ShowEmailReminder { get; set; }
    public int DaysLeft { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                DisplayName = user.UserName ?? string.Empty;
            }
            if (user != null && !user.EmailConfirmed)
            {
                var created = user.CreatedOn;
                if (created != default)
                {
                    var days = 5 - (DateTime.UtcNow - created).Days;
                    if (days > 0)
                    {
                        ShowEmailReminder = true;
                        DaysLeft = days;
                    }
                }
                else
                {
                    ShowEmailReminder = true;
                    DaysLeft = 5;
                }
            }
        }
        else
        {
            // Generate a guest name (e.g., Guest1234)
            var random = new Random();
            DisplayName = $"Guest{random.Next(1000, 9999)}";
        }
    }
}
