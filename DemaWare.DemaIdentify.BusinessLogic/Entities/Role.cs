using DemaWare.DemaIdentify.Models;
using DemaWare.General.Models;
using Microsoft.AspNetCore.Identity;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class Role : IdentityRole<Guid> {
    public RoleModel ToModel() => new() {
        EntityId = Id,
        Name = Name
    };

    public EnumerationModel ToEnumerationModel() => new(Id, Name);
}
