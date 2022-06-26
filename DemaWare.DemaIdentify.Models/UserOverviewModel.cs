using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class UserOverviewModel : EntityModel {
        private string? _email;
        [DataMember]
        public string? Email {
            get { return _email; }
            set { _email = value; NotifyPropertyChanged(); }
        }

        private bool _emailConfirmed;
        [DataMember]
        public bool EmailConfirmed {
            get { return _emailConfirmed; }
            set { _emailConfirmed = value; NotifyPropertyChanged(); }
        }

        private bool _lockoutEnabled;
        [DataMember]
        public bool LockoutEnabled {
            get { return _lockoutEnabled; }
            set { _lockoutEnabled = value; NotifyPropertyChanged(); }
        }

        private DateTime? _lastLoginTime;
        [DataMember]
        public DateTime? LastLoginTime {
            get { return _lastLoginTime; }
            set { _lastLoginTime = value; NotifyPropertyChanged(); }
        }

        [DataMember]
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
