using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.Application.Client;
using DemaWare.DemaIdentify.Models.Application.Scope;
using DemaWare.General.Models;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using System.Text.Json;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class ApplicationService {
    private readonly ILogger<ApplicationService> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ApplicationService(ILogger<ApplicationService> logger, IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager) {
        _logger = logger;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
    }

    #region Clients
    public async Task<ApplicationClientModel> GetClientAsync(Guid clientId) {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

        var client = await _applicationManager.FindByIdAsync(clientId.ToString());
        if (client is null) throw new ArgumentOutOfRangeException(nameof(clientId));

        return ((ApplicationClient)client).ToModel();
    }

    public async Task<IEnumerable<ApplicationClientEnumerationModel>> GetClientEnumerationAsync(bool onlyVisible = false) {
        var clients = await _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Where(x => !onlyVisible || x.IsVisible).Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationClientOverviewModel>> GetClientOverviewAsync() {
        var clients = await _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Select(x => x.ToOverviewModel());
    }

    public async Task SaveClientAsync(ApplicationClientModel clientModel) {
        if (clientModel is null) throw new ArgumentNullException(nameof(clientModel));

        var client = clientModel.IsExistingObject ? await _applicationManager.FindByIdAsync(clientModel.EntityId.ToString()) as ApplicationClient : new ApplicationClient();
        if (client is null) throw new ArgumentOutOfRangeException(nameof(clientModel));

        client.DisplayName = !string.IsNullOrWhiteSpace(clientModel.DisplayName) ? clientModel.DisplayName : null;
        client.ClientId = !string.IsNullOrWhiteSpace(clientModel.ClientId) ? clientModel.ClientId : null;
        client.IsVisible = clientModel.IsVisible;

        client.ConsentType = !string.IsNullOrWhiteSpace(clientModel.ConsentType) ? clientModel.ConsentType : null;
        if (!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) client.ClientSecret = clientModel.ClientSecret;

        client.ApplicationUrl = !string.IsNullOrWhiteSpace(clientModel.ApplicationUrl) ? clientModel.ApplicationUrl : null;
        client.RedirectUris = !string.IsNullOrWhiteSpace(clientModel.RedirectUris) ? JsonSerializer.Serialize(clientModel.RedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;
        client.PostLogoutRedirectUris = !string.IsNullOrWhiteSpace(clientModel.PostLogoutRedirectUris) ? JsonSerializer.Serialize(clientModel.PostLogoutRedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        client.Permissions = !string.IsNullOrWhiteSpace(clientModel.Permissions) ? JsonSerializer.Serialize(clientModel.Permissions.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        if (!clientModel.IsExistingObject) {
			if (!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) await _applicationManager.CreateAsync(client, clientModel.ClientSecret);
			else await _applicationManager.CreateAsync(client);

			_logger.LogInformation("ApplicationClient ({clientId}) added.", clientModel.ClientId);
        } else {
			if (!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) await _applicationManager.UpdateAsync(client, clientModel.ClientSecret);
			else await _applicationManager.UpdateAsync(client);

			_logger.LogInformation("ApplicationClient ({clientId}) changed.", clientModel.ClientId);
        }
    }

    public async Task DeleteClientAsync(Guid clientId) {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

        var client = await _applicationManager.FindByIdAsync(clientId.ToString());
        if (client is null) throw new ArgumentOutOfRangeException(nameof(clientId));
        var clientClientId = ((ApplicationClient)client).ClientId;

        await _applicationManager.DeleteAsync(client);
        _logger.LogInformation("ApplicationClient (({clientClientId})) deleted.", clientClientId);
    }
    #endregion

    #region Scopes
    public async Task<ApplicationScopeModel> GetScopeAsync(Guid scopeId) {
        if (scopeId == Guid.Empty) throw new ArgumentNullException(nameof(scopeId));

		var scope = await _scopeManager.FindByIdAsync(scopeId.ToString());
		if (scope is null) throw new ArgumentOutOfRangeException(nameof(scopeId));

		return ((ApplicationScope)scope).ToModel();
    }

    public async Task<IEnumerable<EnumerationModel>> GetScopeEnumerationAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationScopeOverviewModel>> GetScopeOverviewAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToOverviewModel());
    }

    public async Task SaveScopeAsync(ApplicationScopeModel scopeModel) {
        if (scopeModel is null) throw new ArgumentNullException(nameof(scopeModel));

        var scope = scopeModel.IsExistingObject ? await _scopeManager.FindByIdAsync(scopeModel.EntityId.ToString()) as ApplicationScope : new ApplicationScope();
        if (scope is null) throw new ArgumentOutOfRangeException(nameof(scopeModel));

		scope.Name = !string.IsNullOrWhiteSpace(scopeModel.Name) ? scopeModel.Name : null;
		scope.Resources = !string.IsNullOrWhiteSpace(scopeModel.Resources) ? JsonSerializer.Serialize(scopeModel.Resources.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        if (!scopeModel.IsExistingObject) {
            await _scopeManager.CreateAsync(scope);
            _logger.LogInformation("ApplicationScope ({clientId}) added.", scopeModel.Name);
        } else {
            await _scopeManager.UpdateAsync(scope);
            _logger.LogInformation("ApplicationScope ({clientId}) changed.", scopeModel.Name);
        }
    }

    public async Task DeleteScopeAsync(Guid scopeId) {
        if (scopeId == Guid.Empty) throw new ArgumentNullException(nameof(scopeId));

        var scope = await _scopeManager.FindByIdAsync(scopeId.ToString());
        if (scope is null) throw new ArgumentOutOfRangeException(nameof(scopeId));
        var scopeName = ((ApplicationScope)scope).Name;

        await _scopeManager.DeleteAsync(scope);
        _logger.LogInformation("ApplicationScope (({scopeName})) deleted.", scopeName);
    }
    #endregion
}
