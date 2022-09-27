using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Application.Scope;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application.Scope;
public class ScopeDeleteModel : PageModel {
	private readonly ApplicationService _applicationService;

	[TempData]
    public string? ErrorMessage { get; set; }

    public ApplicationScopeModel? Scope { get; set; }

	public ScopeDeleteModel(ApplicationService applicationService) {
		_applicationService = applicationService;
	}

	public void OnGet(Guid scopeId) {
		Scope = _applicationService.GetScopeAsync(scopeId).Result;
    }

    public async Task<IActionResult> OnPostAsync(Guid scopeId) {
        if (ModelState.IsValid) {
            try {
                await _applicationService.DeleteScopeAsync(scopeId);
                return RedirectToPage("ScopeOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        OnGet(scopeId);
        return Page();
    }
}
