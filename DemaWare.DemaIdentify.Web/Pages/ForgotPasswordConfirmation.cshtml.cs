using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages; 
[AllowAnonymous]
public class ForgotPasswordConfirmation : PageModel {
    public void OnGet() {
    }
}
