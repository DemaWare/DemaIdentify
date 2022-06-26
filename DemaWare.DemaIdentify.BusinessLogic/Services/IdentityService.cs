using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Models;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class IdentityService {
    private readonly ILogger<IdentityService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    private readonly string[] _initialRoles = new string[] { Constants.Roles.SystemAdministrator };

    public IdentityService(ILogger<IdentityService> logger, UserManager<User> userManager, RoleManager<Role> roleManager) {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
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
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, userPassword);

        if (createResult.Succeeded) {
            _logger.LogInformation("User ({emailAddress}) created a new account with password.", userEmail);

            var roleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.SystemAdministrator);
            if (roleResult.Succeeded) {
                _logger.LogInformation("User ({emailAddress}) added to role {roleName}.", userEmail, Constants.Roles.SystemAdministrator);
            }

            foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
        }

        foreach (var error in createResult.Errors) throw new ApplicationException(error.Description);
    }

    public async Task AddUserRoleAsync(Guid userId, string? roleName) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentOutOfRangeException(nameof(userId));

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) for user '{userEmail}' added.", roleName, user.Email);
        foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
    }

    public async Task DeleteUserRoleAsync(Guid userId, string? roleName) {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentOutOfRangeException(nameof(userId));

        var roleResult = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) for user '{userEmail}' deleted.", roleName, user.Email);
        foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
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
        foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
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
        foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
    }

    public async Task DeleteRoleAsync(Guid roleId) {
        if (roleId == Guid.Empty) throw new ArgumentNullException(nameof(roleId));

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) throw new ArgumentOutOfRangeException(nameof(roleId));
        var roleName = role.Name;

        var roleResult = await _roleManager.DeleteAsync(role);
        if (roleResult.Succeeded) _logger.LogInformation("Role ({roleName}) deleted.", roleName);
        foreach (var error in roleResult.Errors) throw new ApplicationException(error.Description);
    }
    #endregion
}
