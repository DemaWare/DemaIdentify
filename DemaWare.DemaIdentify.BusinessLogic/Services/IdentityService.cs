using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Models;
using DemaWare.DemaIdentify.Models.Enums;
using DemaWare.General.Enums;
using DemaWare.General.Helpers;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class IdentityService {
    private readonly ILogger<IdentityService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly EntitiesDbContext _entitiesContext;
    private readonly LocalizationService _localizationService;
    private readonly SettingService _settingService;
    private readonly TemplateService _templateService;

    private readonly string[] _initialRoles = new string[] { Constants.Roles.SystemAdministrator };

    public IdentityService(ILogger<IdentityService> logger, UserManager<User> userManager, RoleManager<Role> roleManager, SignInManager<User> signInManager, EntitiesDbContext entitiesContext, LocalizationService localizationService, SettingService settingService, TemplateService templateService) {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _entitiesContext = entitiesContext;
        _localizationService = localizationService;
        _settingService = settingService;
        _templateService = templateService;
    }

    #region Users
    public async Task<IEnumerable<UserOverviewModel>> GetUserOverviewAsync() {
        var result = new List<UserOverviewModel>();

        foreach (var user in _userManager.Users.OrderBy(x => x.Email).ToList()) {
            var temp = user.ToOverviewModel();
            temp.Roles = await GetUserRolesAsync(user);
            result.Add(temp);
        }

        return result;
    }

    public async Task<UserModel> GetUserAsync(Guid userId) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentOutOfRangeException(nameof(userId));

        var result = user.ToModel();
        result.Roles = await GetUserRolesAsync(user);
        return result;
    }

    private async Task<List<string>> GetUserRolesAsync(User user) {
        return (await _userManager.GetRolesAsync(user)).OrderBy(x => x).ToList();
    }

    public async Task CreateInitialAdminUserAsync(string? userEmail, string? userPassword) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        if (string.IsNullOrEmpty(userPassword)) throw new ArgumentNullException(nameof(userPassword));

        var user = new User {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        var createResult = await _userManager.CreateAsync(user, userPassword);

        if (createResult.Succeeded) {
            _logger.LogInformation("User ({emailAddress}) created a new account with password.", userEmail);

            var roleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.SystemAdministrator);
            if (roleResult.Succeeded) {
                _logger.LogInformation("User ({emailAddress}) added to role {roleName}.", userEmail, Constants.Roles.SystemAdministrator);
            }

            if (roleResult.Errors.Any()) {
                var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
                throw new ApplicationException(error);
            }
        }

        if (createResult.Errors.Any()) {
            var error = string.Join("; ", createResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }

    public async Task PasswordSignInAsync(string? userEmail, string? userPassword) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        if (string.IsNullOrEmpty(userPassword)) throw new ArgumentNullException(nameof(userPassword));
        userEmail = userEmail.ToLower();

        if (!_entitiesContext.Organisations.Any(x => userEmail.Contains(x.DomainName) && x.IsEnabled && !x.IsDeleted))
            throw new ApplicationException(_localizationService.GetLocalizedHtmlString("SignInLimited"));

        var signInResult = await _signInManager.PasswordSignInAsync(userEmail, userPassword, false, lockoutOnFailure: false);

        if (signInResult.Succeeded) {
            var user = await _userManager.FindByEmailAsync(userEmail);
            user.LastLoginTime = DateTime.Now;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User ({userEmail}) logged in.", userEmail.ToLower());

        } else if (signInResult.RequiresTwoFactor) {
            throw new NotImplementedException();
            // TODO: Implement 2FA (#1)
            //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });

        } else if (signInResult.IsLockedOut) {
            throw new NotImplementedException();
            // TODO: Implement account lockout (#4)
            //_logger.LogWarning("User account ({userEmail}) locked out.", Input.Email?.ToLower());
            //return RedirectToPage("./Lockout");

        } else {
            throw new ApplicationException(_localizationService.GetLocalizedHtmlString("ErrorMessageLogin"));
        }
    }

    public async Task SignOutAsync() {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
    }

    public async Task RegisterUserAsync(string? userEmail, string? userPassword, string? confirmEmailUrl) {
        if (string.IsNullOrWhiteSpace(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        if (string.IsNullOrWhiteSpace(userPassword)) throw new ArgumentNullException(nameof(userPassword));
        if (string.IsNullOrWhiteSpace(confirmEmailUrl)) throw new ArgumentNullException(nameof(confirmEmailUrl));
        userEmail = userEmail.ToLower();

        if (!_entitiesContext.Organisations.Any(x => userEmail.Contains(x.DomainName) && x.IsEnabled && !x.IsDeleted))
            throw new ApplicationException(_localizationService.GetLocalizedHtmlString("RegistrationLimited"));

        var user = await _userManager.FindByEmailAsync(userEmail);

        IdentityResult result;
        if (user != null && !await _userManager.HasPasswordAsync(user)) {
            result = await _userManager.AddPasswordAsync(user, userPassword);
        } else {
            user = new User { UserName = userEmail, Email = userEmail };
            result = await _userManager.CreateAsync(user, userPassword);
        }

        if (result.Succeeded) {
            _logger.LogInformation("User ({emailAddress}) created a new account with password.", userEmail);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = string.Format(confirmEmailUrl, user.Id, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)));

            try {
                var templateEmail = _templateService.GenerateEmail(TemplateEmailType.UserRegistrationConfirmEmail);
                templateEmail.Body = templateEmail.Body?.Replace("{email}", userEmail).Replace("{url}", HtmlEncoder.Default.Encode(callbackUrl ?? ""));

                var mailHelper = new MailSmtpHelper(_settingService.GetSmtpSettings());
                mailHelper.AddRecipient(RecipientMailType.To, userEmail);
                mailHelper.Subject = templateEmail.Subject;
                mailHelper.Body = templateEmail.Body;
                mailHelper.IsHtml = true;
                mailHelper.Send();
            } catch (Exception ex) {
                _logger.LogError("The confirmation mail for '{emailAddress}' has failed. {errorMessage}", userEmail, ex.Message);
            }
        }

        if (result.Errors.Any()) {
            var error = string.Join("; ", result.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }

    public async Task ConfirmEmailAsync(Guid userId, string? code) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user != null) {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded) {
                _logger.LogInformation("Email confirmed for user '{userEmail}'.", user.Email);
            } else {
                var error = string.Join("; ", result.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
                _logger.LogError("Unable to confirm user '{userEmail}': {errorMessage} ", user.Email, error);
                throw new ApplicationException(error);
            }
        } else {
            _logger.LogError("ConfirmEmail: Unable to find user with ID '{userId}'.", userId);
            throw new ApplicationException("Unable to find user");
        }
    }

    public async Task SendPasswordResetTokenAsync(string? userEmail, string? resetPasswordUrl) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        if (string.IsNullOrEmpty(resetPasswordUrl)) throw new ArgumentNullException(nameof(resetPasswordUrl));

        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user != null && await _userManager.IsEmailConfirmedAsync(user)) {
            _logger.LogInformation("The user '{userEmail}' has requested a reset password email.", userEmail);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = string.Format(resetPasswordUrl, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email)), WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)));

            try {
                var templateEmail = _templateService.GenerateEmail(TemplateEmailType.UserResetPassword);
                templateEmail.Body = templateEmail.Body?.Replace("{email}", userEmail).Replace("{url}", HtmlEncoder.Default.Encode(callbackUrl ?? ""));

                var mailHelper = new MailSmtpHelper(_settingService.GetSmtpSettings());
                mailHelper.AddRecipient(RecipientMailType.To, user.Email);
                mailHelper.Subject = templateEmail.Subject;
                mailHelper.Body = templateEmail.Body;
                mailHelper.IsHtml = true;
                mailHelper.Send();
            } catch (Exception ex) {
                _logger.LogError("The password reset mail for '{userEmail}' has failed; {errorMessage}", userEmail, ex.Message);
            }
        } else {
            _logger.LogError("The user '{userEmail}' cannot be found to send a reset password email.", userEmail);
            throw new ApplicationException("Unable to find user");
        }
    }

    public async Task ResetPasswordAsync(string? userEmail, string? userPassword, string? code) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        if (string.IsNullOrEmpty(userPassword)) throw new ArgumentNullException(nameof(userPassword));
        if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user != null) {
            var result = await _userManager.ResetPasswordAsync(user, code, userPassword);

            if (result.Succeeded)
                _logger.LogInformation("User ({userEmail}) changed password.", userEmail);

            if (result.Errors.Any()) {
                var error = string.Join("; ", result.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
                _logger.LogError("Unable to reset password for user '{userEmail}': {errorMessage} ", user.Email, error);
                throw new ApplicationException(error);
            }
        } else {
            _logger.LogError("The user '{userEmail}' cannot be found to reset password the password", userEmail);
            throw new ApplicationException("Unable to find user");
        }
    }

    public async Task AddUserRoleAsync(Guid userId, string? roleName) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentOutOfRangeException(nameof(userId));

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) for user '{userEmail}' added.", roleName, user.Email);

        if (roleResult.Errors.Any()) {
            var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }

    public async Task DeleteUserRoleAsync(Guid userId, string? roleName) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentOutOfRangeException(nameof(userId));

        var roleResult = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) for user '{userEmail}' deleted.", roleName, user.Email);

        if (roleResult.Errors.Any()) {
            var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }
    #endregion

    #region Roles
    public async Task<RoleModel> GetRoleAsync(Guid roleId) {
        if (roleId == Guid.Empty) throw new ArgumentNullException(nameof(roleId));
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) throw new ArgumentOutOfRangeException(nameof(roleId));
        return role.ToModel();
    }

    public async Task<IEnumerable<EnumerationModel>> GetRoleEnumerationAsync() {
        var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
        return roles.Select(x => x.ToEnumerationModel());
    }

    public async Task CreateInitialRolesAsync() {
        foreach (var rolName in _initialRoles) {
            if (!await _roleManager.RoleExistsAsync(rolName)) {
                await CreateRoleAsync(new RoleModel { Name = rolName });
            }
        }
    }

    public async Task CreateRoleAsync(RoleModel roleModel) {
        if (roleModel == null || roleModel.EntityId != Guid.Empty || string.IsNullOrEmpty(roleModel.Name))
            throw new ArgumentNullException(nameof(roleModel));

        if (await _roleManager.RoleExistsAsync(roleModel.Name))
            throw new ApplicationException(string.Format("Role {0} is already available!", roleModel.Name));

        var roleResult = await _roleManager.CreateAsync(new Role() {
            Name = roleModel.Name
        });

        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) created.", roleModel.Name);

        if (roleResult.Errors.Any()) {
            var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }

    public async Task EditRoleAsync(RoleModel roleModel) {
        if (roleModel == null || roleModel.EntityId == Guid.Empty || string.IsNullOrEmpty(roleModel.Name))
            throw new ArgumentNullException(nameof(roleModel));

        var role = await _roleManager.FindByIdAsync(roleModel.EntityId.ToString());
        if (role == null) throw new ArgumentOutOfRangeException(nameof(roleModel));

        var oldName = role.Name;
        role.Name = roleModel.Name;

        var roleResult = await _roleManager.UpdateAsync(role);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({oldName} => {roleName}) changed.", oldName, roleModel.Name);

        if (roleResult.Errors.Any()) {
            var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }

    public async Task DeleteRoleAsync(Guid roleId) {
        if (roleId == Guid.Empty) throw new ArgumentNullException(nameof(roleId));

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) throw new ArgumentOutOfRangeException(nameof(roleId));
        var roleName = role.Name;

        var roleResult = await _roleManager.DeleteAsync(role);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) deleted.", roleName);

        if (roleResult.Errors.Any()) {
            var error = string.Join("; ", roleResult.Errors.Select(x => string.Format("{0} - {1}", x.Code, x.Description)));
            throw new ApplicationException(error);
        }
    }
    #endregion
}
