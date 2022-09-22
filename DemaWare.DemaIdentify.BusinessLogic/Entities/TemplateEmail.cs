using DemaWare.DemaIdentify.Enums.Template;
using DemaWare.DemaIdentify.Models.Template;
using DemaWare.General.Extensions;
using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
[Table("TemplateEmail")]
public class TemplateEmail {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
    [ForeignKey("ParentId")]
    public virtual TemplateEmail? Parent { get; set; }

    public TemplateEmailType? Type { get; set; }

    [MaxLength(400)]
    public string? Subject { get; set; }

    [Required]
    public string Body { get; set; } = null!;

    [Required]
    public bool IsDeleted { get; set; }

    public TemplateEmailModel ToModel() => new() {
        EntityId = Id,
        Parent = ParentId,
        Type = Type.HasValue ? (int)Type.Value : null,
        Subject = Subject,
        Body = Body
    };

    public EnumerationModel ToEnumerationModel() => new(Id, Subject ?? string.Empty);

    public TemplateEmailDetailModel ToDetailModel() => new() {
        EntityId = Id,
        Parent = ParentId.HasValue ? Parent?.ToDetailModel() : null,
        Type = Type.HasValue ? Type.Value.ToEnumModel() : null,
        Subject = Subject,
        Body = Body
    };

    public TemplateEmailOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        Type = Type.HasValue ? Type.Value.ToEnumModel() : null,
        Subject = Subject
    };
}
