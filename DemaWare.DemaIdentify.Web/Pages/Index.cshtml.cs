using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.ApplicationClient;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages;
public class IndexModel : PageModel {
    private readonly ApplicationService _applicationService;

    public string? ReturnUrl { get; set; }
    public IEnumerable<ApplicationClientEnumerationModel> ApplicationClients { get; set; } = Enumerable.Empty<ApplicationClientEnumerationModel>();

    public IndexModel(ApplicationService applicationService) {
        _applicationService = applicationService;
    }

    public void OnGet() {
        ReturnUrl = Url.Content("~/");
        ApplicationClients = _applicationService.GetClientEnumerationAsync(onlyVisible: true).Result;
    }
}