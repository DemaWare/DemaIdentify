using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Organisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Organisation; 
public class OrganisationDeleteModel : PageModel {
    private readonly OrganisationService _organisationService;

    [TempData]
    public string? ErrorMessage { get; set; }

    public OrganisationModel? Organisation { get; set; }

    public OrganisationDeleteModel(OrganisationService organisationService) {
        _organisationService = organisationService;
    }

    public void OnGet(Guid organisationId) {
        Organisation = _organisationService.GetOrganisationAsync(organisationId).Result;
    }

    public async Task<IActionResult> OnPostAsync(Guid organisationId) {
        if (ModelState.IsValid) {
            try {
                await _organisationService.DeleteOrganisationAsync(organisationId);
                return RedirectToPage("OrganisationOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        OnGet(organisationId);
        return Page();
    }
}
