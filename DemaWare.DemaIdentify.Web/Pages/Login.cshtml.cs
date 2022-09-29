using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages;
[AllowAnonymous]
public class LoginModel : PageModel {
	private readonly IdentityService _identityService;
	private readonly SettingService _settingService;

	[BindProperty]
	public InputModel Input { get; set; } = new InputModel();

	public IEnumerable<SelectListItem> Languages { get; set; } = new List<SelectListItem>();

	public string? ReturnUrl { get; set; }

	public bool UseDomainCredentials { get; set; }

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

	public LoginModel(IdentityService identityService, SettingService settingService) {
		_identityService = identityService;
		_settingService = settingService;
	}

	public void OnLoad() {
		Languages = LanguageHelper.GenerateLanguageList().Select(c => new SelectListItem { Value = c.Code, Text = c.Name }).ToList();
		UseDomainCredentials = _settingService.UseDomainCredentials;
	}

	public void OnGet(string? returnUrl = null) {
		if (!string.IsNullOrEmpty(ErrorMessage)) ModelState.AddModelError(string.Empty, ErrorMessage);

		ReturnUrl = returnUrl ?? Url.Content("~/");

		// Clear the existing external cookie to ensure a clean login process
		HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).Wait();

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
			try {
				await _identityService.PasswordSignInAsync(Input.Email, Input.Password);
				return LocalRedirect(returnUrl);
			} catch (Exception ex) {
				ModelState.AddModelError(string.Empty, ex.Message);
			}
		}

		OnLoad();
		return Page();
	}
}
