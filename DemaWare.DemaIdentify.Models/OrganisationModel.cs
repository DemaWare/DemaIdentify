using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class OrganisationModel : EntityModel {
        private string? _name;
        [DataMember, Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Name")]
        public string? Name {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        private string? _domainName;
        [DataMember, Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "DomainName")]
        public string? DomainName {
            get { return _domainName; }
            set { _domainName = value; NotifyPropertyChanged(); }
        }

        private bool _isEnabled;
        [DataMember, Display(Name = "Enabled")]
        public bool IsEnabled {
            get { return _isEnabled; }
            set { _isEnabled = value; NotifyPropertyChanged(); }
        }
    }
}
