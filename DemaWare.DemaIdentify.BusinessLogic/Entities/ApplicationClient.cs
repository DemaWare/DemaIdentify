using DemaWare.DemaIdentify.Models;
using OpenIddict.EntityFrameworkCore.Models;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationClient : OpenIddictEntityFrameworkCoreApplication<Guid, ApplicationAuthorization, ApplicationToken> {
    public string? Description { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; }

    public ApplicationClientEnumerationModel ToEnumerationModel() => new() {
        EntityId = Id,
        Name = DisplayName ?? ClientId ?? "n/a",
        Description = Description,
        ApplicationUrl = ApplicationUrl,
        ImageUrl = ImageUrl,
        IsEnabled = IsVisible
    };

    public ApplicationClientOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        ClientId = ClientId,
        DisplayName = DisplayName,
        IsVisible = IsVisible
    };
}
