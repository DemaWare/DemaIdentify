using DemaWare.General.Models;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Template; 
public class TemplateEmailOverviewModel : EntityModel {
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
}
