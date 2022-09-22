using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Application.Scope;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application.Scope; 
public class ScopeOverviewModel : PageModel {
	private readonly ApplicationService _applicationService;

	[TempData]
	public string? ErrorMessage { get; set; }

	public IEnumerable<ApplicationScopeOverviewModel> Scopes { get; set; } = Enumerable.Empty<ApplicationScopeOverviewModel>();

	public ScopeOverviewModel(ApplicationService applicationService) {
		_applicationService = applicationService;
	}

	public void OnGet() {
		Scopes = _applicationService.GetScopeOverviewAsync().Result;
	}
}
