using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application {
    public class ApplicationClientOverviewModel : PageModel {
        private readonly ILogger<ApplicationClientOverviewModel> _logger;
        private readonly ApplicationService _applicationService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<DemaIdentify.Models.ApplicationClientOverviewModel> ApplicationClients { get; set; } = Enumerable.Empty<DemaIdentify.Models.ApplicationClientOverviewModel>();

        public ApplicationClientOverviewModel(ILogger<ApplicationClientOverviewModel> logger, ApplicationService applicationService) {
            _logger = logger;
            _applicationService = applicationService;
        }

        public void OnGet() {
            ApplicationClients = _applicationService.GetClientOverview();
        }
    }
}
