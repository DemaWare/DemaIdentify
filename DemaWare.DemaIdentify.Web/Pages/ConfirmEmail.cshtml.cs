using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel {
        private readonly IdentityService _identityService;

        public string? ReturnUrl { get; set; }

        public ConfirmEmailModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async void OnGet(string? userId, string? code, string? returnUrl) {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(code) && Guid.TryParse(userId, out Guid userIdGuid)) {
                try {
                    await _identityService.ConfirmEmailAsync(userIdGuid, code);
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

        }
    }
}
