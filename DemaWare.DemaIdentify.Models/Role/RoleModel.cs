using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Role; 
public class RoleModel : EntityModel {
    private string? _name;
    [DataMember, Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Role")]
    public string? Name {
        get { return _name; }
        set { _name = value; NotifyPropertyChanged(); }
    }
}
