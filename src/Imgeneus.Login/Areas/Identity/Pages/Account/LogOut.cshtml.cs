#nullable disable

using System.Threading.Tasks;
using Imgeneus.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Areas.Identity.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class LogOutModel : PageModel
    {
        private readonly SignInManager<DbUser> _signInManager;
        private readonly ILogger<LogOutModel> _logger;

        public LogOutModel(SignInManager<DbUser> signInManager, ILogger<LogOutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost()
        {
            if (_signInManager.IsSignedIn(User))
            {
                _logger.LogInformation("User logged out.");
                await _signInManager.SignOutAsync();
            }

            return Redirect("~/");
        }
    }
}
