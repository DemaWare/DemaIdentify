using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application {
    public class ApplicationClientDetailModel : PageModel {
        private readonly ILogger<ApplicationClientDetailModel> _logger;
        private readonly ApplicationService _applicationService;

        public Guid EntityId { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<DemaIdentify.Models.ApplicationClientOverviewModel> ApplicationClients { get; set; } = Enumerable.Empty<DemaIdentify.Models.ApplicationClientOverviewModel>();

        public ApplicationClientDetailModel(ILogger<ApplicationClientDetailModel> logger, ApplicationService applicationService) {
            _logger = logger;
            _applicationService = applicationService;
        }

        public void OnGet(Guid entityId) {
            EntityId = entityId;
            //ApplicationClients = _applicationService.GetClientOverview();
        }
    }
}
