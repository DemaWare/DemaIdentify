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
    public class ForgotPasswordModel : PageModel {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly SettingService _settingService;
        private readonly TemplateService _templateService;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public class InputModel {
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Email")]
            [EmailAddress]
            public string? Email { get; set; }
        }

        public ForgotPasswordModel(UserManager<User> userManager, ILogger<ForgotPasswordModel> logger, SettingService settingService, TemplateService templateService) {
            _userManager = userManager;
            _logger = logger;
            _settingService = settingService;
            _templateService = templateService;
        }

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            if (ModelState.IsValid) {
                _logger.LogInformation("The user '{userEmail}' has requested a reset password email.", Input.Email);

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user)) {
                    _logger.LogError("The user '{userEmail}' cannot be found to send a reset password email.", Input.Email);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/ResetPassword",
                    pageHandler: null,
                    values: new { email = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email)), code, returnUrl },
                    protocol: Request.Scheme);

                try {
                    var templateEmail = _templateService.GenerateEmail(TemplateEmailType.UserResetPassword);
                    templateEmail.Body = templateEmail.Body?.Replace("{email}", Input.Email).Replace("{url}", HtmlEncoder.Default.Encode(callbackUrl ?? ""));

                    var mailHelper = new MailSmtpHelper(_settingService.GetSmtpSettings());
                    mailHelper.AddRecipient(RecipientMailType.To, user.Email);
                    mailHelper.Subject = templateEmail.Subject;
                    mailHelper.Body = templateEmail.Body;
                    mailHelper.IsHtml = true;
                    mailHelper.Send();
                } catch (Exception ex) {
                    _logger.LogError("The password reset mail for '{userEmail}' has failed; {errorMessage}", Input.Email, ex.Message);
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
