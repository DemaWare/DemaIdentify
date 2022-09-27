using DemaWare.General.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemaWare.DemaIdentify.Models.Application.Scope;
public class ApplicationScopeModel : EntityModel {
    private string? _name;
	[DataMember, Display(Name = "Name")]
	public string? Name {
        get { return _name; }
        set { _name = value; NotifyPropertyChanged(); }
    }

	private string? _resources;
	[DataMember, Display(Name = "Resources")]
	public string? Resources {
		get { return _resources; }
		set { _resources = value; NotifyPropertyChanged(); }
	}
}
