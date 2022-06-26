using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class ApplicationClientOverviewModel : EntityModel {
        private string? _clientId;
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

        private string? _description;
        [DataMember]
        public string? Description {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged(); }
        }
    }
}
