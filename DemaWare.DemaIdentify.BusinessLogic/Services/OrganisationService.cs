using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Models.Organisation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class OrganisationService(ILogger<OrganisationService> logger, EntitiesDbContext entitiesContext) {
    public async Task CreateInitialOrganisationAsync(string? userEmail) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        userEmail = userEmail.ToLower();

        if (!entitiesContext.Organisations.Any()) {
            var emailAddress = userEmail.Split("@");

            await entitiesContext.Organisations.AddAsync(new Organisation() {
                Name = "Initial organisation",
                DomainName = emailAddress.Length > 1 ? emailAddress[1] : string.Empty,
                IsEnabled = true,
                IsDeleted = false
            });

            entitiesContext.SaveChanges();
            logger.LogInformation("Inserted initial organisations");
        }
    }

    public async Task<OrganisationModel> GetOrganisationAsync(Guid organisationId) {
        if (organisationId == Guid.Empty) throw new ArgumentNullException(nameof(organisationId));
        var organisation = await entitiesContext.Organisations.FindAsync(organisationId);
        if (organisation == null || organisation.IsDeleted) throw new ArgumentOutOfRangeException(nameof(organisationId));
        return organisation.ToModel();
    }

    public async Task<IEnumerable<OrganisationOverviewModel>> GetOrganisationOverviewAsync() {
        var organisations = await entitiesContext.Organisations.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
        return organisations.Select(x => x.ToOverviewModel());
    }

    public async Task SaveOrganisationAsync(OrganisationModel organisationModel) {
        ArgumentNullException.ThrowIfNull(organisationModel);

        var organisation = organisationModel.IsExistingObject ? await entitiesContext.Organisations.FirstAsync(x => x.Id == organisationModel.EntityId && !x.IsDeleted) ?? throw new ArgumentOutOfRangeException(nameof(organisationModel)) : new Organisation();
        if (!organisationModel.IsExistingObject) entitiesContext.Organisations.Add(organisation);

        organisation.Name = !string.IsNullOrWhiteSpace(organisationModel.Name) ? organisationModel.Name : string.Empty;
        organisation.DomainName = !string.IsNullOrWhiteSpace(organisationModel.DomainName) ? organisationModel.DomainName.ToLower() : string.Empty;
        organisation.IsEnabled = organisationModel.IsEnabled;

        await entitiesContext.SaveChangesAsync();
        logger.LogInformation("Organisation ({organisationName}) changed.", organisation.Name);
    }

    public async Task DeleteOrganisationAsync(Guid organisationId) {
        if (organisationId == Guid.Empty) throw new ArgumentNullException(nameof(organisationId));
        var organisation = await entitiesContext.Organisations.FindAsync(organisationId);
        if (organisation == null || organisation.IsDeleted) throw new ArgumentOutOfRangeException(nameof(organisationId));

        organisation.IsDeleted = true;

        await entitiesContext.SaveChangesAsync();
        logger.LogInformation("Organisation ({organisationName}) deleted.", organisation.Name);
    }
}
