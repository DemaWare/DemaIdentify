using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class RegisterModel : PageModel {
        private readonly IdentityService _identityService;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        // TODO: Implement external providers (#3)
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel {
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Email")]
            [EmailAddress]
            public string? Email { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Password")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Confirm password")]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        public RegisterModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // TODO: Implement external providers
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            OnGet(returnUrl);

            if (ModelState.IsValid) {
                try {
                    var confirmEmailUrl = Url.Page("/ConfirmEmail", new { userId = "{0}", code = "{1}", returnUrl = ReturnUrl });
                    await _identityService.RegisterUserAsync(Input.Email, Input.Password, confirmEmailUrl);
                    return RedirectToPage("RegisterConfirmation");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return Page();
        }
    }
}
