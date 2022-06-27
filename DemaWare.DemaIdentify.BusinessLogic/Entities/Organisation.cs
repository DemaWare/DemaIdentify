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

    //public TemplateEmailModel ToModel() => new() {
    //    EntityId = Id,
    //    Parent = ParentId,
    //    Type = Type.HasValue ? (int)Type.Value : null,
    //    Subject = Subject,
    //    Body = Body
    //};

    //public EnumerationModel ToEnumerationModel() => new EnumerationModel(Id, Subject ?? string.Empty);

    //public TemplateEmailDetailModel ToDetailModel() => new() {
    //    EntityId = Id,
    //    Parent = ParentId.HasValue ? Parent?.ToDetailModel() : null,
    //    Type = Type.HasValue ? Type.Value.ToEnumModel() : null,
    //    Subject = Subject,
    //    Body = Body
    //};

    //public TemplateEmailOverviewModel ToOverviewModel() => new() {
    //    EntityId = Id,
    //    Type = Type.HasValue ? Type.Value.ToEnumModel() : null,
    //    Subject = Subject
    //};
}
