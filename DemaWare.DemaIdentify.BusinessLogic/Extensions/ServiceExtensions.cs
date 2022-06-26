using DemaWare.DemaIdentify.BusinessLogic.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DemaWare.DemaIdentify.BusinessLogic.Extensions;
public static class ServiceExtensions {
    public static IServiceCollection AddDemaIdentify(this IServiceCollection services, string connectionString) {
        // Add EntityFramework
        services.AddDbContext<EntitiesDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        // Add services
        services.AddTransient<ApplicationService>();
        services.AddTransient<IdentityService>();
        services.AddTransient<SettingService>();
        services.AddTransient<TemplateService>();

        return services;
    }
}
