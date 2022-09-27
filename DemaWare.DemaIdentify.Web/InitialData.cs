using DemaWare.DemaIdentify.BusinessLogic;
using Microsoft.EntityFrameworkCore;

namespace DemaWare.DemaIdentify.Web;
public class InitialData : IHostedService {
    private readonly IServiceProvider _serviceProvider;

    public InitialData(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        using var scope = _serviceProvider.CreateScope();

        // Apply any pending migration in or create the database
        using var entitiesContext = scope.ServiceProvider.GetRequiredService<EntitiesDbContext>();
        await entitiesContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
