using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DemaWare.DemaIdentify.Web.Pages; 
[AllowAnonymous]
public class ResetPasswordModel : PageModel {
    private readonly IdentityService _identityService;

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ReturnUrl { get; set; }

    public class InputModel {
        [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "EmailAddress")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "ErrorMessageLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "PasswordNoMatch")]
        public string? ConfirmPassword { get; set; }

        public string? Code { get; set; }
    }

    public ResetPasswordModel(IdentityService identityService) {
        _identityService = identityService;
    }

    public IActionResult OnGet(string email, string code, string? returnUrl = null) {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code)) 
            return BadRequest("A code must be supplied for password reset.");

        ReturnUrl = returnUrl;
        Input = new InputModel {
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
            Email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email))
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
        if (ModelState.IsValid) {
            try {
                await _identityService.ResetPasswordAsync(Input.Email, Input.Password, Input.Code);
            } catch {
                // Do nothing, always redirect to confirmation page
            }

            return RedirectToPage("./ResetPasswordConfirmation", new { returnUrl });
        }

        return Page();

    }
}
