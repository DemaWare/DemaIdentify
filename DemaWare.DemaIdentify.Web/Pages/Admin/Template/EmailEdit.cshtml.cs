using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using DemaWare.DemaIdentify.Models.Enums;
using DemaWare.General.Extensions;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template {
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

        public async void OnGet(Guid templateId) {
            Input = await _templateService.GetEmailAsync(templateId);
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
}
