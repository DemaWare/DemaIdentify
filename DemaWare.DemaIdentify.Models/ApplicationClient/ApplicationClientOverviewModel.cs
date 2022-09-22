using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.ApplicationClient; 
public class ApplicationClientOverviewModel : EntityModel {
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

    private bool _isVisible;
    [DataMember]
    public bool IsVisible {
        get { return _isVisible; }
        set { _isVisible = value; NotifyPropertyChanged(); }
    }
}
