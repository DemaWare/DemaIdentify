using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Application.Client;
public class ApplicationClientOverviewModel : EntityModel {
    private string? _clientId = null!;
	[DataMember, Display(Name = "ClientId")]
	public string? ClientId {
        get { return _clientId; }
        set { _clientId = value; NotifyPropertyChanged(); }
    }

    private string? _displayName;
    [DataMember, Display(Name = "Name")]
    public string? DisplayName {
        get { return _displayName; }
        set { _displayName = value; NotifyPropertyChanged(); }
    }

    private bool _isVisible;
    [DataMember, Display(Name = "Visible")]
    public bool IsVisible {
        get { return _isVisible; }
        set { _isVisible = value; NotifyPropertyChanged(); }
    }
}
