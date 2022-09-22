using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity
{
    public class RoleCreateModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public RoleModel Input { get; set; } = new();

        public RoleCreateModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async Task<IActionResult> OnPost() {
            if (ModelState.IsValid) {
                try {
                    await _identityService.CreateRoleAsync(Input);
                    return RedirectToPage("RoleOverview");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return Page();
        }
    }
}
