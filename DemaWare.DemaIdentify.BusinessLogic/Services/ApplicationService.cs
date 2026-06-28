using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.Application.Client;
using DemaWare.DemaIdentify.Models.Application.Scope;
using DemaWare.General.Models;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using System.Text.Json;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;

public class ApplicationService(ILogger<ApplicationService> logger, IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager) {
    #region Clients
    public async Task<ApplicationClientModel> GetClientAsync(Guid clientId) {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

        var client = await applicationManager.FindByIdAsync(clientId.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(clientId));
        return ((ApplicationClient)client).ToModel();
    }

    public async Task<IEnumerable<ApplicationClientEnumerationModel>> GetClientEnumerationAsync(bool onlyVisible = false) {
        var clients = await applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Where(x => !onlyVisible || x.IsVisible).Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationClientOverviewModel>> GetClientOverviewAsync() {
        var clients = await applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Select(x => x.ToOverviewModel());
    }

    public async Task SaveClientAsync(ApplicationClientModel clientModel) {
        ArgumentNullException.ThrowIfNull(clientModel);

        var client = clientModel.IsExistingObject ? await applicationManager.FindByIdAsync(clientModel.EntityId.ToString()) as ApplicationClient ?? throw new ArgumentOutOfRangeException(nameof(clientModel)) : new ApplicationClient();
        client.DisplayName = !string.IsNullOrWhiteSpace(clientModel.DisplayName) ? clientModel.DisplayName : null;
        client.ClientId = !string.IsNullOrWhiteSpace(clientModel.ClientId) ? clientModel.ClientId : null;
        client.IsVisible = clientModel.IsVisible;

        client.ConsentType = !string.IsNullOrWhiteSpace(clientModel.ConsentType) ? clientModel.ConsentType : null;

        client.ApplicationUrl = !string.IsNullOrWhiteSpace(clientModel.ApplicationUrl) ? clientModel.ApplicationUrl : null;
        client.RedirectUris = !string.IsNullOrWhiteSpace(clientModel.RedirectUris) ? JsonSerializer.Serialize(clientModel.RedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;
        client.PostLogoutRedirectUris = !string.IsNullOrWhiteSpace(clientModel.PostLogoutRedirectUris) ? JsonSerializer.Serialize(clientModel.PostLogoutRedirectUris.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        client.Permissions = !string.IsNullOrWhiteSpace(clientModel.Permissions) ? JsonSerializer.Serialize(clientModel.Permissions.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        if (!clientModel.IsExistingObject) {
            if (!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) await applicationManager.CreateAsync(client, clientModel.ClientSecret);
            else await applicationManager.CreateAsync(client);

            logger.LogInformation("ApplicationClient ({clientId}) added.", clientModel.ClientId);
        } else {
            if (!string.IsNullOrWhiteSpace(clientModel.ClientSecret)) await applicationManager.UpdateAsync(client, clientModel.ClientSecret);
            else await applicationManager.UpdateAsync(client);

            logger.LogInformation("ApplicationClient ({clientId}) changed.", clientModel.ClientId);
        }
    }

    public async Task DeleteClientAsync(Guid clientId) {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

        var client = await applicationManager.FindByIdAsync(clientId.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(clientId));
        var clientClientId = ((ApplicationClient)client).ClientId;

        await applicationManager.DeleteAsync(client);
        logger.LogInformation("ApplicationClient (({clientClientId})) deleted.", clientClientId);
    }
    #endregion

    #region Scopes
    public async Task<ApplicationScopeModel> GetScopeAsync(Guid scopeId) {
        if (scopeId == Guid.Empty) throw new ArgumentNullException(nameof(scopeId));

        var scope = await scopeManager.FindByIdAsync(scopeId.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(scopeId));
        return ((ApplicationScope)scope).ToModel();
    }

    public async Task<IEnumerable<EnumerationModel>> GetScopeEnumerationAsync() {
        var scopes = await scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationScopeOverviewModel>> GetScopeOverviewAsync() {
        var scopes = await scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToOverviewModel());
    }

    public async Task SaveScopeAsync(ApplicationScopeModel scopeModel) {
        ArgumentNullException.ThrowIfNull(scopeModel);

        var scope = (scopeModel.IsExistingObject ? await scopeManager.FindByIdAsync(scopeModel.EntityId.ToString()) as ApplicationScope : new ApplicationScope()) ?? throw new ArgumentOutOfRangeException(nameof(scopeModel));
        scope.Name = !string.IsNullOrWhiteSpace(scopeModel.Name) ? scopeModel.Name : null;
        scope.Resources = !string.IsNullOrWhiteSpace(scopeModel.Resources) ? JsonSerializer.Serialize(scopeModel.Resources.Split(";").Where(x => !string.IsNullOrWhiteSpace(x))) : null;

        if (!scopeModel.IsExistingObject) {
            await scopeManager.CreateAsync(scope);
            logger.LogInformation("ApplicationScope ({clientId}) added.", scopeModel.Name);
        } else {
            await scopeManager.UpdateAsync(scope);
            logger.LogInformation("ApplicationScope ({clientId}) changed.", scopeModel.Name);
        }
    }

    public async Task DeleteScopeAsync(Guid scopeId) {
        if (scopeId == Guid.Empty) throw new ArgumentNullException(nameof(scopeId));

        var scope = await scopeManager.FindByIdAsync(scopeId.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(scopeId));
        var scopeName = ((ApplicationScope)scope).Name;

        await scopeManager.DeleteAsync(scope);
        logger.LogInformation("ApplicationScope (({scopeName})) deleted.", scopeName);
    }
    #endregion
}
