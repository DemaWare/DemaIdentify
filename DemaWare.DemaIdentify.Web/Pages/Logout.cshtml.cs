using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class LogoutModel : PageModel {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LogoutModel(SignInManager<User> signInManager, ILogger<LoginModel> logger) {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(string? returnUrl = null) {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            if (returnUrl != null) return LocalRedirect(returnUrl);
            return RedirectToPage("/Index");
        }
    }
}