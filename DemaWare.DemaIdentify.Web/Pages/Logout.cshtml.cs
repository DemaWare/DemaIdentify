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

        public async Task<IActionResult> OnGet(string? returnUrl = null) {
            try {
                await _identityService.SignOutAsync();
                if (returnUrl != null) return LocalRedirect(returnUrl);
                return RedirectToPage("/Index");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}