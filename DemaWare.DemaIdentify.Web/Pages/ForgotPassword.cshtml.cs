using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel {
        private readonly IdentityService _identityService;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public class InputModel {
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Email")]
            [EmailAddress]
            public string? Email { get; set; }
        }

        public ForgotPasswordModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            OnGet(returnUrl);

            if (ModelState.IsValid) {
                try {
                    var resetPasswordUrl = Url.Page("/ResetPassword", null, new { email = "{0}", code = "{1}", returnUrl = ReturnUrl }, Request.Scheme);
                    await _identityService.SendPasswordResetTokenAsync(Input.Email, resetPasswordUrl);
                } catch {
                    // Do nothing, always redirect to confirmation page
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
