using DemaWare.DemaIdentify.BusinessLogic;
using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DemaWare.DemaIdentify.Web; 
public class InitialData : IHostedService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InitialData> _logger;

    public InitialData(IServiceProvider serviceProvider, ILogger<InitialData> logger) {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        using var scope = _serviceProvider.CreateScope();

        // Apply any pending migration in or create the database
        using var entitiesContext = scope.ServiceProvider.GetRequiredService<EntitiesDbContext>();
        await entitiesContext.Database.MigrateAsync(cancellationToken);
        
        //TODO: Implement in UI for Admin role
        // OpenIddict
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await applicationManager.FindByClientIdAsync("DemaIdentify_Admin", cancellationToken) is null) {
            await applicationManager.CreateAsync(new ApplicationClient {
                ClientId = "DemaIdentify_Admin",
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "DemaIdentify (Admin)",
                ApplicationUrl = "https://localhost:44368/",
                RedirectUris = JsonSerializer.Serialize(new string[] { "https://localhost:44368/authentication/login-callback" }),
                PostLogoutRedirectUris = JsonSerializer.Serialize(new string[] { "https://localhost:44368/authentication/logout-callback" }),
                IsVisible = false,
                Permissions = JsonSerializer.Serialize(new string[] { 
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,

                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Prefixes.Scope + "api_demaidentify_admin",
                })
            }, cancellationToken);
            _logger.Log(LogLevel.Information, "OpenIddict: Client 'DemaIdentify_Admin' added!");
        }

        if (await applicationManager.FindByClientIdAsync("api://DemaIdentify_Admin", cancellationToken) is null) {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor {
                ClientId = "api://DemaIdentify_Admin",
                ClientSecret = "C655E74D-4BE3-43C0-B520-02C0D3DDDD84",
                Permissions = {
                    Permissions.Endpoints.Introspection,
                }
            }, cancellationToken); ;
            _logger.Log(LogLevel.Information, "OpenIddict: Client 'api://DemaIdentify_Admin' added!");
        }

        if (await scopeManager.FindByNameAsync("api_demaidentify_admin", cancellationToken) is null) {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor {
                Name = "api_demaidentify_admin",
                Resources =  {
                    "api://DemaIdentify_Admin"
                }
            }, cancellationToken);
            _logger.Log(LogLevel.Information, "OpenIddict: Scope 'api_demaidentify_admin' added!");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
