using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity.Role; 
public class RoleDeleteModel : PageModel {
	private readonly IdentityService _identityService;

	[TempData]
	public string? ErrorMessage { get; set; }

	public RoleModel? Role { get; set; }

	public RoleDeleteModel(IdentityService identityService) {
		_identityService = identityService;
	}

	public void OnGet(Guid roleId) {
		Role = _identityService.GetRoleAsync(roleId).Result;
	}

	public async Task<IActionResult> OnPostAsync(Guid roleId) {
		if (ModelState.IsValid) {
			try {
				await _identityService.DeleteRoleAsync(roleId);
				return RedirectToPage("RoleOverview");
			} catch (Exception ex) {
				ModelState.AddModelError(string.Empty, ex.Message);
			}
		}

		OnGet(roleId);
		return Page();
	}
}
