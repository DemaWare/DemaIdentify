using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application {
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
}
