using System.ComponentModel.DataAnnotations;

namespace DemaWare.DemaIdentify.Models.Enums {
    public enum TemplateEmailType {
        [Display(Order = 0)]
        UserRegistrationConfirmEmail = 1,

        [Display(Order = 1)]
        UserResetPassword = 2
    }
}
