using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.User;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity
{
    public class UserRolesModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public UserModel Input { get; set; } = new();

        public SelectList RoleList { get; set; } = null!;

        [BindProperty]
        public string? RoleNameAdd { get; set; }

        public UserRolesModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public void OnGet(Guid userId) {
            Input = _identityService.GetUserAsync(userId).Result;

            var roles = _identityService.GetRoleEnumerationAsync().Result;
            RoleList = new SelectList(roles, nameof(EnumerationModel.Name), nameof(EnumerationModel.Name));
        }

        public async Task<IActionResult> OnPostAsync(Guid userId) {
            if (!string.IsNullOrWhiteSpace(RoleNameAdd)) {
                if (ModelState.IsValid) {
                    try {
                        await _identityService.AddUserRoleAsync(userId, RoleNameAdd);
                        return RedirectToPage("./UserRoles", new { userId = userId });
                    } catch (Exception ex) {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }

            OnGet(userId);
            return Page();
        }
    }
}
