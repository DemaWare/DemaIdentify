using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Template {
    public class EmailOverviewModel : PageModel {
        private readonly TemplateService _templateService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<TemplateEmailOverviewModel> Templates { get; set; } = Enumerable.Empty<TemplateEmailOverviewModel>();

        public EmailOverviewModel(TemplateService templateService) {
            _templateService = templateService;
        }

        public async void OnGet() {
            Templates = await _templateService.GetEmailOverviewAsync();
        }
    }
}
