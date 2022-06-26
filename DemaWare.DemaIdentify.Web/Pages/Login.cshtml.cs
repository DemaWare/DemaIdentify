using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Web.Helpers;
using DemaWare.DemaIdentify.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class LoginModel : PageModel {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly LocalizationService _localizer;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public IEnumerable<SelectListItem> Languages { get; set; } = new List<SelectListItem>();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel {
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Language")]
            public string? Language { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Email")]
            [EmailAddress]
            public string? Email { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Password")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }

        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<LoginModel> logger, LocalizationService localizer) {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _localizer = localizer;
        }

        public void OnLoad() {
            Languages = LanguageHelper.GenerateLanguageList().Select(c => new SelectListItem { Value = c.Code, Text = c.Name }).ToList();
        }

        public async Task OnGetAsync(string? returnUrl = null) {
            if (!string.IsNullOrEmpty(ErrorMessage)) ModelState.AddModelError(string.Empty, ErrorMessage);

            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            if (requestCultureFeature != null) {
                var currentCulture = requestCultureFeature.RequestCulture.UICulture.Name;
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(currentCulture)), new CookieOptions { IsEssential = true, Expires = DateTimeOffset.UtcNow.AddYears(1) });
                Input.Language = currentCulture;
            }

            OnLoad();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid) {
                // TODO: Implement account lockout (#4)
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, lockoutOnFailure: false);
                if (result.Succeeded) {
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    user.LastLoginTime = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User ({userEmail}) logged in.", Input.Email?.ToLower());
                    return LocalRedirect(returnUrl);

                } else if (result.RequiresTwoFactor) {
                    throw new NotImplementedException();
                    // TODO: Implement 2FA (#1)
                    //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });

                } else if (result.IsLockedOut) {
                    throw new NotImplementedException();
                    // TODO: Implement account lockout (#4)
                    //_logger.LogWarning("User account ({userEmail}) locked out.", Input.Email?.ToLower());
                    //return RedirectToPage("./Lockout");

                } else {
                    ModelState.AddModelError(string.Empty, _localizer.GetLocalizedHtmlString("ErrorMessageLogin"));
                }
            }

            OnLoad();
            return Page();
        }
    }
}
