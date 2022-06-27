using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using DemaWare.DemaIdentify.Models.Enums;
using DemaWare.General.Extensions;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template {
    public class EmailCreateModel : PageModel {
        private readonly TemplateService _templateService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public SelectList ParentList { get; set; } = null!;
        public SelectList TypeList { get; set; } = null!;

        [BindProperty]
        public TemplateEmailModel Input { get; set; } = new();

        public EmailCreateModel(TemplateService templateService) {
            _templateService = templateService;
        }

        public async void OnGet() {
            var templates = (await _templateService.GetEmailEnumerationAsync()).ToList();
            ParentList = new SelectList(templates, nameof(EnumerationModel.EntityId), nameof(EnumerationModel.Name));
            
            var types = EnumExtension.ToEnumList<TemplateEmailType>();
            TypeList = new SelectList(types, nameof(EnumModel.Id), nameof(EnumModel.Name));
        }

        public async Task<IActionResult> OnPostAsync() {
            if (ModelState.IsValid) {
                try {
                    await _templateService.SaveEmailAsync(Input);
                    return RedirectToPage("EmailOverview");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            OnGet();
            return Page();
        }
    }
}
