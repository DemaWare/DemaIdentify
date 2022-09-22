using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Enums.Template;
public enum TemplateEmailType {
    [Display(Order = 0)]
    UserRegistrationConfirmEmail = 1,

    [Display(Order = 1)]
    UserResetPassword = 2
}
