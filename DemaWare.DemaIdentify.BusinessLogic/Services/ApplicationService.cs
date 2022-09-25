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

    public async Task<IEnumerable<ApplicationClientEnumerationModel>> GetClientEnumerationAsync(bool onlyVisible) {
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

        client.DisplayName = !string.IsNullOrWhiteSpace(clientModel.ClientId) ? clientModel.DisplayName : null;
        client.ClientId = !string.IsNullOrWhiteSpace(clientModel.ClientId) ? clientModel.ClientId : null;
        client.IsVisible = clientModel.IsVisible;

        client.ConsentType = !string.IsNullOrWhiteSpace(clientModel.ConsentType) ? clientModel.ConsentType : null;
        if(!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) client.ClientSecret = clientModel.ClientSecret;

        client.ApplicationUrl = !string.IsNullOrWhiteSpace(clientModel.ApplicationUrl) ? clientModel.ApplicationUrl : null;
        client.RedirectUris = !string.IsNullOrWhiteSpace(clientModel.RedirectUris) ? JsonSerializer.Serialize(clientModel.RedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;
        client.PostLogoutRedirectUris = !string.IsNullOrWhiteSpace(clientModel.PostLogoutRedirectUris) ? JsonSerializer.Serialize(clientModel.PostLogoutRedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        client.Permissions = !string.IsNullOrWhiteSpace(clientModel.Permissions) ? JsonSerializer.Serialize(clientModel.Permissions.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        if (!clientModel.IsExistingObject) {
            await _applicationManager.CreateAsync(client);
            _logger.LogInformation("ApplicationClient ({clientId}) added.", clientModel.ClientId);
        } else {
            await _applicationManager.UpdateAsync(client);
            _logger.LogInformation("ApplicationClient ({clientId}) changed.", clientModel.ClientId);
        }
    }

    public async Task DeleteClientAsync(Guid clientId) {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

        var client = await _applicationManager.FindByIdAsync(clientId.ToString());
        if (client is null) throw new ArgumentOutOfRangeException(nameof(clientId));
        var clientModel = ((ApplicationClient)client).ToModel();

        await _applicationManager.DeleteAsync(client);
        _logger.LogInformation("ApplicationClient (({clientId})) deleted.", clientModel.ClientId);
    }
    #endregion

    #region Scopes
    public async Task<IEnumerable<EnumerationModel>> GetScopeEnumerationAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationScopeOverviewModel>> GetScopeOverviewAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToOverviewModel());
    }
    #endregion
}
