using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.ApplicationClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Application
{
    public class ClientOverviewModel : PageModel {
        private readonly ApplicationService _applicationService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<ApplicationClientOverviewModel> Clients { get; set; } = Enumerable.Empty<ApplicationClientOverviewModel>();

        public ClientOverviewModel(ApplicationService applicationService) {
            _applicationService = applicationService;
        }

        public void OnGet() {
            Clients = _applicationService.GetClientOverviewAsync().Result;
        }
    }
}
