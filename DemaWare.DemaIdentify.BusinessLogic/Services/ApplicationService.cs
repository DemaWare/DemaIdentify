using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models;
using OpenIddict.Abstractions;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class ApplicationService {
    private readonly IOpenIddictApplicationManager _applicationManager;

    public ApplicationService(IOpenIddictApplicationManager applicationManager) {
        _applicationManager = applicationManager;
    }

    public IEnumerable<ApplicationClientEnumerationModel> GetClientEnumeration(bool onlyVisible) {
        return _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync().Result
         .Cast<ApplicationClient>()
         .Where(x => !onlyVisible || x.IsVisible).ToList()
         .Select(x => x.ToEnumerationModel());
    }

    public IEnumerable<ApplicationClientOverviewModel> GetClientOverview() {
        return _applicationManager.ListAsync(x => x, CancellationToken.None).ToListAsync().Result
         .Cast<ApplicationClient>()
         .ToList()
         .Select(x => x.ToOverviewModel());
    }
}
