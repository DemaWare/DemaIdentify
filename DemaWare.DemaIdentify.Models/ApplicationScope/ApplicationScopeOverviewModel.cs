using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.ApplicationScope;
public class ApplicationScopeOverviewModel : EntityModel {
    private string? _name;
    [DataMember]
    public string? Name {
        get { return _name; }
        set { _name = value; NotifyPropertyChanged(); }
    }

    [DataMember]
    public IEnumerable<string> Resources { get; set; } = Enumerable.Empty<string>();
}
