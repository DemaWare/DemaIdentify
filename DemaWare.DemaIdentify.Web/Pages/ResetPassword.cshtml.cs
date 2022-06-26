using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

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

        public ResetPasswordModel(UserManager<User> userManager, ILogger<ResetPasswordModel> logger) {
            _userManager = userManager;
            _logger = logger;
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
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null) return RedirectToPage("./ResetPasswordConfirmation", new { returnUrl });

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded) return RedirectToPage("./ResetPasswordConfirmation", new { returnUrl });

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
    }
}
