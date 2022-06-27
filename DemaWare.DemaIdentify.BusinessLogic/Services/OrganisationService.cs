using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class OrganisationService {
    private readonly ILogger<OrganisationService> _logger;
    private readonly EntitiesDbContext _entitiesContext;

    public OrganisationService(ILogger<OrganisationService> logger, EntitiesDbContext entitiesContext) {
        _logger = logger;
        _entitiesContext = entitiesContext;
    }

    public async Task CreateInitialOrganisationAsync(string? userEmail) {
        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));
        
        if (!_entitiesContext.Organisations.Any()) {
            var emailAddress = userEmail.Split("@");

            await _entitiesContext.Organisations.AddAsync(new Organisation() {
                Name = "Initial organisation",
                DomainName = emailAddress.Length > 1 ? emailAddress[1] : string.Empty,
                IsEnabled = true,
                IsDeleted = false
            });

            _entitiesContext.SaveChanges();
            _logger.LogInformation("Inserted initial organisations");
        }
    }

    public async Task<OrganisationModel> GetOrganisationAsync(Guid organisationId) {
        if (organisationId == Guid.Empty) throw new ArgumentNullException(nameof(organisationId));
        var organisation = await _entitiesContext.Organisations.FindAsync(organisationId);
        if (organisation == null || organisation.IsDeleted) throw new ArgumentOutOfRangeException(nameof(organisationId));
        return organisation.ToModel();
    }

    public async Task<IEnumerable<OrganisationOverviewModel>> GetOrganisationOverviewAsync() {
        var organisations = await _entitiesContext.Organisations.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
        return organisations.Select(x => x.ToOverviewModel());
    }

    public async Task SaveOrganisationAsync(OrganisationModel organisationModel) {
        if (organisationModel == null) throw new ArgumentNullException(nameof(organisationModel));

        var organisation = organisationModel.IsExistingObject ? await _entitiesContext.Organisations.FirstAsync(x => x.Id == organisationModel.EntityId && !x.IsDeleted) : new Organisation();
        if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationModel));
        if (!organisationModel.IsExistingObject) _entitiesContext.Organisations.Add(organisation);

        organisation.Name = !string.IsNullOrWhiteSpace(organisationModel.Name) ? organisationModel.Name : string.Empty;
        organisation.DomainName = !string.IsNullOrWhiteSpace(organisationModel.DomainName) ? organisationModel.DomainName : string.Empty;
        organisation.IsEnabled = organisationModel.IsEnabled;        

        await _entitiesContext.SaveChangesAsync();
        _logger.LogInformation("Organisation ({organisationName}) changed.", organisation.Name);
    }

    public async Task DeleteOrganisationAsync(Guid organisationId) {
        if (organisationId == Guid.Empty) throw new ArgumentNullException(nameof(organisationId));
        var organisation = await _entitiesContext.Organisations.FindAsync(organisationId);
        if (organisation == null || organisation.IsDeleted) throw new ArgumentOutOfRangeException(nameof(organisationId));

        organisation.IsDeleted = true;

        await _entitiesContext.SaveChangesAsync();
        _logger.LogInformation("Organisation ({organisationName}) deleted.", organisation.Name);
    }
}
