using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models {
    public class TemplateEmailDetailModel : EntityModel {
        private TemplateEmailDetailModel? _parent;
        [DataMember]
        public TemplateEmailDetailModel? Parent {
            get { return _parent; }
            set { _parent = value; NotifyPropertyChanged(); }
        }

        private EnumModel? _type;
        [DataMember]
        public EnumModel? Type {
            get { return _type; }
            set { _type = value; NotifyPropertyChanged(); }
        }

        private string? _subject;
        [DataMember]
        public string? Subject {
            get { return _subject; }
            set { _subject = value; NotifyPropertyChanged(); }
}
        private string? _body;
        [DataMember]
        public string? Body {
            get { return _body; }
            set { _body = value; NotifyPropertyChanged(); }
        }

        #region Properties (public)
        public void MergeTemplate() {
            _body = MergeBodyWithParent();
        }

        private string? MergeBodyWithParent() {
            return _parent != null ? _parent.MergeBodyWithParent()?.Replace("{content}", _body) : _body;
        }
        #endregion
    }
}
