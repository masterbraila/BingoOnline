using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BingoGameOnline.Server.Models;

namespace BingoGameOnline.Server.Api
{
    [Route("api/account")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();
            return Ok(new { userId = user.Id, userName = user.UserName });
        }

        [HttpGet("displayname")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisplayName([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new { displayName = user.UserName });
        }
    }
}
