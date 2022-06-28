using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class OrganisationOverviewModel : EntityModel {
        private string _name = null!;
        [DataMember]
        public string Name {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        private string _domainName = null!;
        [DataMember]
        public string DomainName {
            get { return _domainName; }
            set { _domainName = value; NotifyPropertyChanged(); }
        }

        private bool _isEnabled;
        [DataMember]
        public bool IsEnabled {
            get { return _isEnabled; }
            set { _isEnabled = value; NotifyPropertyChanged(); }
        }
    }
}
