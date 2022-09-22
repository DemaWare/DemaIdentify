using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity
{
    public class UserRoleDeleteModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public UserModel Input { get; set; } = new();
        public string? RoleName { get; set; }

        public UserRoleDeleteModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public void OnGet(Guid userId, string roleName) {
            Input = _identityService.GetUserAsync(userId).Result;
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
