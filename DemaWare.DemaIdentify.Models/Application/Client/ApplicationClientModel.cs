using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Application.Client;
public class ApplicationClientModel : EntityModel {
    private string? _displayName;
    [DataMember, Display(Name = "Name")]
    public string? DisplayName {
        get { return _displayName; }
        set { _displayName = value; NotifyPropertyChanged(); }
    }

    private string? _clientId = null!;
    [DataMember, Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "ClientId")]
    public string? ClientId {
        get { return _clientId; }
        set { _clientId = value; NotifyPropertyChanged(); }
    }

    private bool _isVisible;
    [DataMember, Display(Name = "Visible")]
    public bool IsVisible {
        get { return _isVisible; }
        set { _isVisible = value; NotifyPropertyChanged(); }
    }

    #region Security
    private string? _consentType;
    [DataMember, Display(Name = "ConsentType")]
    public string? ConsentType {
        get { return _consentType; }
        set { _consentType = value; NotifyPropertyChanged(); }
    }

    private string? _clientSecret;
    [DataMember, Display(Name = "ClientSecret")]
    public string? ClientSecret {
        get { return _clientSecret; }
        set { _clientSecret = value; NotifyPropertyChanged(); }
    }
    #endregion

    #region URLs
    private string? _applicationUrl;
    [DataMember, Display(Name = "ApplicationUrl")]
    public string? ApplicationUrl {
        get { return _applicationUrl; }
        set { _applicationUrl = value; NotifyPropertyChanged(); }
    }

    private string? _redirectUris;
    [DataMember, Display(Name = "RedirectUris")]
    public string? RedirectUris {
        get { return _redirectUris; }
        set { _redirectUris = value; NotifyPropertyChanged(); }
    }

    private string? _postLogoutRedirectUris;
    [DataMember, Display(Name = "PostLogoutRedirectUris")]
    public string? PostLogoutRedirectUris {
        get { return _postLogoutRedirectUris; }
        set { _postLogoutRedirectUris = value; NotifyPropertyChanged(); }
    }
    #endregion

    #region Permissions
    private string? _permissions;
    [DataMember, Display(Name = "Permissions")]
    public string? Permissions {
        get { return _permissions; }
        set { _permissions = value; NotifyPropertyChanged(); }
    }
    #endregion
}
