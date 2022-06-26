using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DemaWare.DemaIdentify.Web.Pages {
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        [TempData]
        public bool IsMailConfirmed { get; set; }

        public string? ReturnUrl { get; set; }

        public ConfirmEmailModel(UserManager<User> userManager, ILogger<ConfirmEmailModel> logger) {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl) {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (userId == null || !Guid.TryParse(userId, out Guid userIdGuid) || code == null) return RedirectToPage("/Login");

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
            if (user == null) {
                _logger.LogError("ConfirmEmail: Unable to load user with ID '{userId}'.", userId);
                return Page();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded) {
                IsMailConfirmed = true;
                _logger.LogInformation("Email confirmed for user '{userEmail}'.", user.Email);
            } else {
                _logger.LogError("Unable to confirm user '{userEmail}'.{errorMessage}", user.Email, string.Join(";", result.Errors.Select(x => string.Format(" {0} - {1}", x.Code, x.Description))));
            }

            return Page();
        }
    }
}
