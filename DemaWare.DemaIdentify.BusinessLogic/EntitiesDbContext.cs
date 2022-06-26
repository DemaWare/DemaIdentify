using DemaWare.DemaIdentify.BusinessLogic.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemaWare.DemaIdentify.BusinessLogic;
public class EntitiesDbContext : IdentityDbContext<User, Role, Guid> {
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<TemplateEmail> TemplateEmails => Set<TemplateEmail>();

    public EntitiesDbContext(DbContextOptions<EntitiesDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<ApplicationClient, ApplicationAuthorization, ApplicationScope, ApplicationToken, Guid>();
    }
}
