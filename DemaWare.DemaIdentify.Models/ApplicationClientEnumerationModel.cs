using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class ApplicationClientEnumerationModel : EnumerationModel {
        private string? _description;
        [DataMember]
        public string? Description {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged(); }
        }

        private string? _applicationUrl;
        [DataMember]
        public string? ApplicationUrl {
            get { return _applicationUrl; }
            set { _applicationUrl = value; NotifyPropertyChanged(); }
        }

        private string? _imageUrl;
        [DataMember]
        public string? ImageUrl {
            get { return _imageUrl; }
            set { _imageUrl = value; NotifyPropertyChanged(); }
        }
    }
}
