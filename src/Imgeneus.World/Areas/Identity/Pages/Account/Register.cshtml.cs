using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Imgeneus.World.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/Account/Login");
        }
    }
}
