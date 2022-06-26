using DemaWare.DemaIdentify.Models;
using Microsoft.AspNetCore.Identity;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class User : IdentityUser<Guid> {
    public DateTime? LastLoginTime { get; set; }

    public UserOverviewModel ToOverviewModel() => new() {
        EntityId = Id,
        Email = Email,
        EmailConfirmed = EmailConfirmed,
        LockoutEnabled = LockoutEnabled,
        LastLoginTime = LastLoginTime
    };

    public UserModel ToModel() => new() {
        EntityId = Id,
        Email = Email,
        EmailConfirmed = EmailConfirmed,
        LockoutEnabled = LockoutEnabled,
        LastLoginTime = LastLoginTime
    };
}
