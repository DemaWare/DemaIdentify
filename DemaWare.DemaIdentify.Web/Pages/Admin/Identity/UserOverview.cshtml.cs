using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemaWare.DemaIdentify.Web.Pages.Admin.Identity {
    public class UserOverviewModel : PageModel {
        private readonly IdentityService _identityService;

        [TempData]
        public string? ErrorMessage { get; set; }

        public IEnumerable<DemaIdentify.Models.UserOverviewModel> Users { get; set; } = Enumerable.Empty<DemaIdentify.Models.UserOverviewModel>();

        public UserOverviewModel(IdentityService identityService) {
            _identityService = identityService;
        }

        public async void OnGet() {
            Users = await _identityService.GetUserOverviewAsync();
        }
    }
}
