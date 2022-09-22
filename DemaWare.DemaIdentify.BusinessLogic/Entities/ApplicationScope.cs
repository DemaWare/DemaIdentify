using DemaWare.DemaIdentify.Models.Application.Scope;
using OpenIddict.EntityFrameworkCore.Models;
using System.Text.Json;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationScope : OpenIddictEntityFrameworkCoreScope<Guid> {
    public ApplicationScopeOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        Name = Name,
        Resources = JsonSerializer.Deserialize<IEnumerable<string>>(Resources ?? "[]") ?? Enumerable.Empty<string>()
    };
}
