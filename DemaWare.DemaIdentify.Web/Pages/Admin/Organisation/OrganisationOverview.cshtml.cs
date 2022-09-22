using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Organisation; 
public class OrganisationOverviewModel : PageModel {
    private readonly OrganisationService _organisationService;

    [TempData]
    public string? ErrorMessage { get; set; }

    public IEnumerable<DemaIdentify.Models.Organisation.OrganisationOverviewModel> Organisations { get; set; } = Enumerable.Empty<DemaIdentify.Models.Organisation.OrganisationOverviewModel>();

    public OrganisationOverviewModel(OrganisationService organisationService) {
        _organisationService = organisationService;
    }

    public void OnGet() {
        Organisations = _organisationService.GetOrganisationOverviewAsync().Result;
    }
}
