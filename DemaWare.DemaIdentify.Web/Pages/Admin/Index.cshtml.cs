using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Web.Pages.Admin {
    public class IndexModel : PageModel {
        private readonly SettingService _settingService;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel {
            /* General settings */
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
        }

        public IndexModel(SettingService settingService) {
            _settingService = settingService;
        }

        public void OnGet() {
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
        }

        public void OnPost() {
            if (ModelState.IsValid) {
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
            }
        }
    }
}
