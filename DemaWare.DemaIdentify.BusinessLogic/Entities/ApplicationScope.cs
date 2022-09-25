using DemaWare.DemaIdentify.Models.Application.Scope;
using DemaWare.General.Models;
using OpenIddict.EntityFrameworkCore.Models;
using System.Text.Json;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationScope : OpenIddictEntityFrameworkCoreScope<Guid> {

	public ApplicationScopeModel ToModel() => new() {
		EntityId = Id,
		Name = Name,
		Resources = string.Join(";", JsonSerializer.Deserialize<string[]>(Resources ?? "[]") ?? Array.Empty<string>())
	};

	public EnumerationModel ToEnumerationModel() => new(Id, Name ?? string.Empty);

	public ApplicationScopeOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        Name = Name,
        Resources = JsonSerializer.Deserialize<IEnumerable<string>>(Resources ?? "[]") ?? Enumerable.Empty<string>()
    };
}
