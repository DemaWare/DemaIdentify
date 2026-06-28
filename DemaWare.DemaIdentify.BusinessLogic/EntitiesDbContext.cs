using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemaWare.DemaIdentify.BusinessLogic;
public class EntitiesDbContext(DbContextOptions<EntitiesDbContext> options) : IdentityDbContext<User, Role, Guid>(options) {
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<TemplateEmail> TemplateEmails => Set<TemplateEmail>();

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<ApplicationClient, ApplicationAuthorization, ApplicationScope, ApplicationToken, Guid>();
    }
}
