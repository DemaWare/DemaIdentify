using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity {
    public class RoleEditModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public RoleModel Input { get; set; } = new();

        public RoleEditModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async void OnGet(Guid roleId) {
            Input = await _identityService.GetRoleAsync(roleId);
        }

        public async Task<IActionResult> OnPostAsync() {
            if (ModelState.IsValid) {
                try {
                    await _identityService.EditRoleAsync(Input);
                    return RedirectToPage("RoleOverview");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return Page();
        }
    }
}
