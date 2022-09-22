using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.User; 
public class UserModel : EntityModel {
    private string? _email;
    [DataMember, Display(Name = "EmailAddress")]
    public string? Email {
        get { return _email; }
        set { _email = value; NotifyPropertyChanged(); }
    }

    private bool _emailConfirmed;
    [DataMember, Display(Name = "EmailAddressConfirmed")]
    public bool EmailConfirmed {
        get { return _emailConfirmed; }
        set { _emailConfirmed = value; NotifyPropertyChanged(); }
    }

    private bool _lockoutEnabled;
    [DataMember, Display(Name = "LockoutEnabled")]
    public bool LockoutEnabled {
        get { return _lockoutEnabled; }
        set { _lockoutEnabled = value; NotifyPropertyChanged(); }
    }

    private DateTime? _lastLoginTime;
    [DataMember, Display(Name = "LastLoginTime")]
    public DateTime? LastLoginTime {
        get { return _lastLoginTime; }
        set { _lastLoginTime = value; NotifyPropertyChanged(); }
    }

    [DataMember, Display(Name = "Roles")]
    public List<string> Roles { get; set; } = new List<string>();
}
