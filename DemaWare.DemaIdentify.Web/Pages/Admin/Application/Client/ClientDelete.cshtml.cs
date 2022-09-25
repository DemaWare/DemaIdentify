using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Application.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application.Client;
public class ClientDeleteModel : PageModel {
	private readonly ApplicationService _applicationService;

	[TempData]
    public string? ErrorMessage { get; set; }

    public ApplicationClientModel? Client { get; set; }

	public ClientDeleteModel(ApplicationService applicationService) {
		_applicationService = applicationService;
	}

	public void OnGet(Guid clientId) {
        Client = _applicationService.GetClientAsync(clientId).Result;
    }

    public async Task<IActionResult> OnPostAsync(Guid clientId) {
        if (ModelState.IsValid) {
            try {
                await _applicationService.DeleteClientAsync(clientId);
                return RedirectToPage("ClientOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        OnGet(clientId);
        return Page();
    }
}
