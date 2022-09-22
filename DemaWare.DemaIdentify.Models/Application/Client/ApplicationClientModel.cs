using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Application.Client; 
public class ApplicationClientModel : EntityModel {
    private string? _clientId = null!;
    [DataMember]
    public string? ClientId {
        get { return _clientId; }
        set { _clientId = value; NotifyPropertyChanged(); }
    }

    private string? _displayName;
    [DataMember]
    public string? DisplayName {
        get { return _displayName; }
        set { _displayName = value; NotifyPropertyChanged(); }
    }
}
