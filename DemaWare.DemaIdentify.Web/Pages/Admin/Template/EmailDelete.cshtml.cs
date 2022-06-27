using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template {
    public class EmailDeleteModel : PageModel {
        private readonly TemplateService _templateService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public TemplateEmailModel? Template { get; set; }

        public EmailDeleteModel(TemplateService templateService) {
            _templateService = templateService;
        }

        public async void OnGet(Guid templateId) {
            Template = await _templateService.GetEmailAsync(templateId);
        }

        public async Task<IActionResult> OnPostAsync(Guid templateId) {
            if (ModelState.IsValid) {
                try {
                    await _templateService.DeleteEmailAsync(templateId);
                    return RedirectToPage("EmailOverview");
                } catch (Exception ex) {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            OnGet(templateId);
            return Page();
        }
    }
}
