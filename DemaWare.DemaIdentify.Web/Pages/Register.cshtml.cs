using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Enums;
using DemaWare.General.Enums;
using DemaWare.General.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class RegisterModel : PageModel {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly SettingService _settingService;
        private readonly TemplateService _templateService;

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

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RegisterModel> logger, SettingService settingService, TemplateService templateService) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _settingService = settingService;
            _templateService = templateService;
        }

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl;
            // TODO: Implement external providers (#3)
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            returnUrl ??= Url.Content("~/");

            // TODO: Implement external providers (#3)
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                IdentityResult result;
                if (user != null && !await _userManager.HasPasswordAsync(user)) {
                    result = await _userManager.AddPasswordAsync(user, Input.Password);
                } else {
                    user = new User { UserName = Input.Email, Email = Input.Email };
                    result = await _userManager.CreateAsync(user, Input.Password);
                }

                if (result.Succeeded) {
                    _logger.LogInformation("User ({emailAddress}) created a new account with password.", Input.Email);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code, returnUrl },
                        protocol: Request.Scheme);

                    try {
                        var templateEmail = _templateService.GenerateEmail(TemplateEmailType.UserRegistrationConfirmEmail);
                        templateEmail.Body = templateEmail.Body?.Replace("{email}", Input.Email).Replace("{url}", HtmlEncoder.Default.Encode(callbackUrl ?? ""));

                        var mailHelper = new MailSmtpHelper(_settingService.GetSmtpSettings());
                        mailHelper.AddRecipient(RecipientMailType.To, Input.Email ?? string.Empty);
                        mailHelper.Subject = templateEmail.Subject;
                        mailHelper.Body = templateEmail.Body;
                        mailHelper.IsHtml = true;
                        mailHelper.Send();
                    } catch (Exception ex) {
                        _logger.LogError("The confirmation mail for '{emailAddress}' has failed. {errorMessage}", Input.Email, ex.Message);
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                        return RedirectToPage("RegisterConfirmation");
                    } else {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
