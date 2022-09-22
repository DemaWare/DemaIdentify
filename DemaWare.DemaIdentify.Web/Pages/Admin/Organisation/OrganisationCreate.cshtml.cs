using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.DemaIdentify.Models.Organisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Organisation; 
public class OrganisationCreateModel : PageModel {
    private readonly OrganisationService _organisationService;

    [TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public OrganisationModel Input { get; set; } = new();

    public OrganisationCreateModel(OrganisationService organisationService) {
        _organisationService = organisationService;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (ModelState.IsValid) {
            try {
                await _organisationService.SaveOrganisationAsync(Input);
                return RedirectToPage("OrganisationOverview");
            } catch (Exception ex) {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        return Page();
    }
}
