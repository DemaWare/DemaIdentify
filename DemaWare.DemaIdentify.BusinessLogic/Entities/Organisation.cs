using DemaWare.DemaIdentify.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
[Table("Organisation")]
public class Organisation {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string DomainName { get; set; } = null!;

    [Required]
    public bool IsEnabled { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    public OrganisationModel ToModel() => new() {
        EntityId = Id,
        Name = Name,
        DomainName = DomainName,
        IsEnabled = IsEnabled
    };

    public OrganisationOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        Name = Name,
        DomainName = DomainName,
        IsEnabled = IsEnabled
    };
}
