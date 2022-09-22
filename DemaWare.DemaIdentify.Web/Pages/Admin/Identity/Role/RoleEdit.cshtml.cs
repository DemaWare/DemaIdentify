using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity.Role; 
public class RoleEditModel : PageModel {
	private readonly IdentityService _identityService;

	[TempData]
	public string? ErrorMessage { get; set; }

	[BindProperty]
	public RoleModel Input { get; set; } = new();

	public RoleEditModel(IdentityService identityService) {
		_identityService = identityService;
	}

	public void OnGet(Guid roleId) {
		Input = _identityService.GetRoleAsync(roleId).Result;
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
