using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Enums.Setting;
using DemaWare.DemaIdentify.Web.Helpers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages {
    public class ConfigureModel : PageModel {
        private readonly ILogger<ConfigureModel> _logger;
        private readonly IdentityService _identityService;
        private readonly OrganisationService _organisationService;
        private readonly TemplateService _templateService;
        private readonly SettingService _settingService;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IEnumerable<SelectListItem> Languages { get; set; } = new List<SelectListItem>();

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel {
            /* General settings */
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Language")]
            public string? Language { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "ApplicationName")]
            public string? ApplicationName { get; set; }

            /* Default colors */
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "ColorBaseBackground")]
            public string? ColorBaseBackground { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "ColorBaseForeground")]
            public string? ColorBaseForeground { get; set; }

            /* External images */
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "UrlLogoWhite")]
            public string? UrlLogoWhite { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "UrlLogoColor")]
            public string? UrlLogoColor { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "UrlBackgroundCover")]
            public string? UrlBackgroundCover { get; set; }

            /* Email settings */
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "SmtpHost")]
            public string? SmtpHost { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "SmtpPort")]
            public int SmtpPort { get; set; }

            [Display(Name = "SmtpEnableSsl")]
            public bool SmtpEnableSsl { get; set; }

            [Display(Name = "SmtpUsername")]
            public string? SmtpUsername { get; set; }

            [Display(Name = "SmtpPassword")]
            public string? SmtpPassword { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "SmtpFromAddress")]
            public string? SmtpFromAddress { get; set; }

            [Display(Name = "SmtpFromName")]
            public string? SmtpFromName { get; set; }

            /* Admin user */
            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "EmailAddress")]
            [EmailAddress]
            public string? UserEmail { get; set; }

            [Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Password")]
            [DataType(DataType.Password)]
            public string? UserPassword { get; set; }
        }

        public ConfigureModel(ILogger<ConfigureModel> logger, IdentityService identityService, OrganisationService organisationService, TemplateService templateService, SettingService settingService) {
            _logger = logger;
            _identityService = identityService;
            _organisationService = organisationService;
            _templateService = templateService;
            _settingService = settingService;
        }

        public void OnLoad() {
            Languages = LanguageHelper.GenerateLanguageList().Select(c => new SelectListItem { Value = c.Code, Text = c.Name }).ToList();
        }

        public IActionResult OnGet() {
            if (_settingService.IsApplicationConfigured) return RedirectToPage("Index");

            var smtpSettings = _settingService.GetSmtpSettings();

            Input = new() {
                ApplicationName = _settingService.ApplicationName,

                ColorBaseBackground = _settingService.ColorBaseBackground,
                ColorBaseForeground = _settingService.ColorBaseForeground,

                UrlLogoWhite = _settingService.UrlLogoWhite,
                UrlLogoColor = _settingService.UrlLogoColor,
                UrlBackgroundCover = _settingService.UrlBackgroundCover,

                SmtpHost = smtpSettings.Host,
                SmtpPort = smtpSettings.Port,
                SmtpEnableSsl = smtpSettings.EnableSsl,
                SmtpUsername = smtpSettings.Username,
                SmtpPassword = smtpSettings.Password,
                SmtpFromAddress = smtpSettings.FromAddress,
                SmtpFromName = smtpSettings.FromName
            };

            var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            if (requestCultureFeature != null) {
                var currentCulture = requestCultureFeature.RequestCulture.UICulture.Name;
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(currentCulture)), new CookieOptions { IsEssential = true, Expires = DateTimeOffset.UtcNow.AddYears(1) });
                Input.Language = currentCulture;
            }

            OnLoad();
            return Page();
        }

        public async Task<IActionResult> OnPost() {
            if (ModelState.IsValid) {

                try {
                    using var trans = _settingService.BeginTransaction();

                    // Save all settings into the database
                    _settingService.Save(SettingType.ApplicationName, Input.ApplicationName);

                    _settingService.Save(SettingType.ColorBaseBackground, Input.ColorBaseBackground);
                    _settingService.Save(SettingType.ColorBaseForeground, Input.ColorBaseForeground);

                    _settingService.Save(SettingType.UrlLogoWhite, Input.UrlLogoWhite);
                    _settingService.Save(SettingType.UrlLogoColor, Input.UrlLogoColor);
                    _settingService.Save(SettingType.UrlBackgroundCover, Input.UrlBackgroundCover);

                    _settingService.Save(SettingType.SmtpHost, Input.SmtpHost);
                    _settingService.Save(SettingType.SmtpPort, Input.SmtpPort);
                    _settingService.Save(SettingType.SmtpEnableSsl, Input.SmtpEnableSsl);
                    _settingService.Save(SettingType.SmtpUsername, Input.SmtpUsername);
                    _settingService.Save(SettingType.SmtpPassword, Input.SmtpPassword);
                    _settingService.Save(SettingType.SmtpFromAddress, Input.SmtpFromAddress);
                    _settingService.Save(SettingType.SmtpFromName, Input.SmtpFromName);

                    await _identityService.CreateInitialRolesAsync();
                    //TODO: Make it optional, based on setting (#7)
                    await _identityService.CreateInitialAdminUserAsync(Input.UserEmail, Input.UserPassword);
                    await _organisationService.CreateInitialOrganisationAsync(Input.UserEmail); //TODO: 
                    await _templateService.CreateInitialTemplatesAsync();

                    trans.Commit();
                    _logger.Log(LogLevel.Information, "Initial settings saved to DB");
                    return RedirectToPage("Index");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            OnLoad();
            return Page();
        }
    }
}
