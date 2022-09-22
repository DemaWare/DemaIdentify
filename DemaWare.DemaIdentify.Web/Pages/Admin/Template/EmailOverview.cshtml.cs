using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Template;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template; 
public class EmailOverviewModel : PageModel {
	private readonly TemplateService _templateService;

	[TempData]
	public string? ErrorMessage { get; set; }

	public IEnumerable<TemplateEmailOverviewModel> Templates { get; set; } = Enumerable.Empty<TemplateEmailOverviewModel>();

	public EmailOverviewModel(TemplateService templateService) {
		_templateService = templateService;
	}

	public void OnGet() {
		Templates = _templateService.GetEmailOverviewAsync().Result;
	}
}
