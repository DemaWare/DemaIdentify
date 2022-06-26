using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel {
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }
    }
}
