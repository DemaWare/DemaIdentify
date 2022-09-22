using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.ApplicationClient;
using DemaWare.DemaIdentify.Models.ApplicationScope;
using OpenIddict.Abstractions;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class ApplicationService {
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ApplicationService(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager) {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
    }

    #region Clients
    public async Task<IEnumerable<ApplicationClientEnumerationModel>> GetClientEnumerationAsync(bool onlyVisible) {
        var clients = await _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Where(x => !onlyVisible || x.IsVisible).Select(x => x.ToEnumerationModel());
    }

    public async Task<IEnumerable<ApplicationClientOverviewModel>> GetClientOverviewAsync() {
        var clients = await _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync();
        return clients.Cast<ApplicationClient>().Select(x => x.ToOverviewModel());
    }
    #endregion

    #region Scopes
    public async Task<IEnumerable<ApplicationScopeOverviewModel>> GetScopeOverviewAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToOverviewModel());
    }
    #endregion
}
