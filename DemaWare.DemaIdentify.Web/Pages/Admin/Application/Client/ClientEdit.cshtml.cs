using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Application.Client;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application.Client; 
public class ClientEditModel : PageModel {
	private readonly ApplicationService _applicationService;

	[TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public ApplicationClientModel Input { get; set; } = new();

    public IEnumerable<EnumerationModel> Scopes { get; private set; } = new List<EnumerationModel>();

    public ClientEditModel(ApplicationService applicationService) {
		_applicationService = applicationService;
	}

	public void OnGet(Guid clientId) {
        Input = _applicationService.GetClientAsync(clientId).Result;
        Scopes = _applicationService.GetScopeEnumerationAsync().Result;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (ModelState.IsValid) {
            try {
                await _applicationService.SaveClientAsync(Input);
                return RedirectToPage("ClientOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return Page();
    }
}
