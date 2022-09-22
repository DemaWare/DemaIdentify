using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.Application.Client;
using DemaWare.DemaIdentify.Models.Application.Scope;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

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
        if (client == null) throw new ArgumentOutOfRangeException(nameof(clientId));
        
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

	public async Task DeleteClientAsync(Guid clientId) {
		if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));

		var client = await _applicationManager.FindByIdAsync(clientId.ToString());
		if (client == null) throw new ArgumentOutOfRangeException(nameof(clientId));
        var clientModel = ((ApplicationClient)client).ToModel();
		
        await _applicationManager.DeleteAsync(client);
		_logger.LogInformation("ApplicationClient (({clientId})) deleted.", clientModel.ClientId);
	}
	#endregion

	#region Scopes
	public async Task<IEnumerable<ApplicationScopeOverviewModel>> GetScopeOverviewAsync() {
        var scopes = await _scopeManager.ListAsync().ToListAsync();
        return scopes.Cast<ApplicationScope>().Select(x => x.ToOverviewModel());
    }
    #endregion
}
