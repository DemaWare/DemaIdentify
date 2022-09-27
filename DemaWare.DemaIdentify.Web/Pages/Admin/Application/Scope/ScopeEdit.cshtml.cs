using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Application.Scope;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application.Scope; 
public class ScopeEditModel : PageModel {
	private readonly ApplicationService _applicationService;

	[TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public ApplicationScopeModel Input { get; set; } = new();

    public IEnumerable<EnumerationModel> Clients { get; private set; } = new List<EnumerationModel>();

    public ScopeEditModel(ApplicationService applicationService) {
		_applicationService = applicationService;
	}

	public void OnGet(Guid scopeId) {
        Input = _applicationService.GetScopeAsync(scopeId).Result;
        Clients = _applicationService.GetClientEnumerationAsync().Result;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (ModelState.IsValid) {
            try {
                await _applicationService.SaveScopeAsync(Input);
                return RedirectToPage("ScopeOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return Page();
    }
}
