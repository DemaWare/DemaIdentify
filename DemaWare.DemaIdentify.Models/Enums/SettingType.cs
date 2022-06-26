using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Models.Enums {
    public enum SettingType {
        [Display(Order = 0, Name = "ApplicationName")]
        ApplicationName = 1,

        /* Default colors */
        [Display(Order = 1, Name = "ColorBaseBackground")]
        ColorBaseBackground = 2,

        [Display(Order = 2, Name = "ColorBaseForeground")]
        ColorBaseForeground = 3,

        /* External images */
        [Display(Order = 3, Name = "UrlLogoWhite")]
        UrlLogoWhite = 4,
        
        [Display(Order = 4, Name = "UrlLogoColor")]
        UrlLogoColor = 5,

        [Display(Order = 5, Name = "UrlBackgroundCover")]
        UrlBackgroundCover = 6,

        /* Email configuration */
        [Display(Order = 6, Name = "SmtpHost")]
        SmtpHost = 7,
        
        [Display(Order = 7, Name = "SmtpPort")]
        SmtpPort = 8,
        
        [Display(Order = 8, Name = "SmtpEnableSsl")]
        SmtpEnableSsl = 9,
        
        [Display(Order = 9, Name = "SmtpUsername")]
        SmtpUsername = 10,
        
        [Display(Order = 10, Name = "SmtpPassword")]
        SmtpPassword = 11,
        
        [Display(Order = 11, Name = "SmtpFromAddress")]
        SmtpFromAddress = 12,
        
        [Display(Order = 12, Name = "SmtpFromName")]
        SmtpFromName = 13
    }
}
