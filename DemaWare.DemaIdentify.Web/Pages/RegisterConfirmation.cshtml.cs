using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel {

        public IActionResult OnGet() {
            return Page();
        }
    }
}