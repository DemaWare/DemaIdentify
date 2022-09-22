using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Enums.Template;
using DemaWare.DemaIdentify.Models.Template;
using DemaWare.General.Extensions;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template; 
public class EmailEditModel : PageModel {
	private readonly TemplateService _templateService;

	[TempData]
	public string? ErrorMessage { get; set; }

	public SelectList ParentList { get; set; } = null!;
	public SelectList TypeList { get; set; } = null!;

	[BindProperty]
	public TemplateEmailModel Input { get; set; } = new();

	public EmailEditModel(TemplateService templateService) {
		_templateService = templateService;
	}

	public void OnGet(Guid templateId) {
		Input = _templateService.GetEmailAsync(templateId).Result;
		LoadData(templateId);
	}

	public async Task<IActionResult> OnPostAsync(Guid templateId) {
		if (ModelState.IsValid) {
			try {
				await _templateService.SaveEmailAsync(Input);
				return RedirectToPage("EmailOverview");
			} catch (Exception ex) {
				ModelState.AddModelError(string.Empty, ex.Message);
			}
		}

		LoadData(templateId);
		return Page();
	}

	private void LoadData(Guid templateId) {
		var templates = _templateService.GetEmailEnumerationAsync().Result.ToList();
		templates.Remove(templates.First(x => x.EntityId == templateId));
		ParentList = new SelectList(templates, nameof(EnumerationModel.EntityId), nameof(EnumerationModel.Name));

		var types = EnumExtension.ToEnumList<TemplateEmailType>();
		TypeList = new SelectList(types, nameof(EnumModel.Id), nameof(EnumModel.Name));
	}
}
