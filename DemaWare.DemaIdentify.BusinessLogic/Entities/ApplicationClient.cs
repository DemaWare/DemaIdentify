using DemaWare.DemaIdentify.Models.Application.Client;
using OpenIddict.EntityFrameworkCore.Models;
using System.Text.Json;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationClient : OpenIddictEntityFrameworkCoreApplication<Guid, ApplicationAuthorization, ApplicationToken> {
    public string? Description { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; }

    public ApplicationClientModel ToModel() => new() {
        EntityId = Id,
        DisplayName = DisplayName,
        ClientId = ClientId,
        IsVisible = IsVisible,

        ConsentType = ConsentType,
        ClientSecret = null,

        ApplicationUrl = ApplicationUrl,
        RedirectUris = string.Join(";", JsonSerializer.Deserialize<string[]>(RedirectUris ?? "[]") ?? Array.Empty<string>()),
        PostLogoutRedirectUris = string.Join(";", JsonSerializer.Deserialize<string[]>(PostLogoutRedirectUris ?? "[]") ?? Array.Empty<string>()),
        
        Permissions = string.Join(";", JsonSerializer.Deserialize<string[]>(Permissions ?? "[]") ?? Array.Empty<string>())
    };

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
