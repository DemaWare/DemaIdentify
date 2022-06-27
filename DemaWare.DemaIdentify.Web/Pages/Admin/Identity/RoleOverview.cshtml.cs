using DemaWare.DemaIdentify.BusinessLogic.Services;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity {
    public class RoleOverviewModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<EnumerationModel> Roles { get; set; } = Enumerable.Empty<EnumerationModel>();

        public RoleOverviewModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async void OnGet() {
            Roles = await _identityService.GetRoleEnumerationAsync();
        }
    }
}
