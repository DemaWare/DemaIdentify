using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class LogoutModel : PageModel {
        private readonly IdentityService _identityService;

        public LogoutModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public  IActionResult OnGet(string? returnUrl = null) {
            try {
                _identityService.SignOutAsync().Wait();
                if (returnUrl != null) return LocalRedirect(returnUrl);
                return RedirectToPage("/Index");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}