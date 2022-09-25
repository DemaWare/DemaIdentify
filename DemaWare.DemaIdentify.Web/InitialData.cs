using DemaWare.DemaIdentify.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

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
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

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
