using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity {
    public class UserRoleDeleteModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public UserModel Input { get; set; } = new();
        public string? RoleName { get; set; }

        public UserRoleDeleteModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async void OnGet(Guid userId, string roleName) {
            Input = await _identityService.GetUserAsync(userId);
            RoleName = roleName;
        }

        public async Task<IActionResult> OnPostAsync(Guid userId, string roleName) {
            if (ModelState.IsValid) {
                try {
                    await _identityService.DeleteUserRoleAsync(userId, roleName);
                    return RedirectToPage("./UserRoles", new { userId = userId });
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            OnGet(userId, roleName);
            return Page();
        }
    }
}
