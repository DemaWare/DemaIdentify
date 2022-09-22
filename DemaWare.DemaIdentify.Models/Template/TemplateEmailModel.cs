using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Template;
public class TemplateEmailModel : EntityModel {
    private Guid? _parent;
    [DataMember, Display(Name = "Parent")]
    public Guid? Parent {
        get { return _parent; }
        set { _parent = value; NotifyPropertyChanged(); }
    }

    private int? _type;
    [DataMember, Display(Name = "Type")]
    public int? Type {
        get { return _type; }
        set { _type = value; NotifyPropertyChanged(); }
    }

    private string? _subject;
    [DataMember, Required(ErrorMessage = "ErrorMessageRequired"), Display(Name = "Subject")]
    public string? Subject {
        get { return _subject; }
        set { _subject = value; NotifyPropertyChanged(); }
    }
    private string? _body;
    [DataMember, Display(Name = "Body")]
    public string? Body {
        get { return _body; }
        set { _body = value; NotifyPropertyChanged(); }
    }
}
