using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity.User; 
public class UserOverviewModel : PageModel {
	private readonly IdentityService _identityService;

	[TempData]
	public string? ErrorMessage { get; set; }

	public IEnumerable<DemaIdentify.Models.User.UserOverviewModel> Users { get; set; } = Enumerable.Empty<DemaIdentify.Models.User.UserOverviewModel>();

	public UserOverviewModel(IdentityService identityService) {
		_identityService = identityService;
	}

	public void OnGet() {
		Users = _identityService.GetUserOverviewAsync().Result;
	}
}
