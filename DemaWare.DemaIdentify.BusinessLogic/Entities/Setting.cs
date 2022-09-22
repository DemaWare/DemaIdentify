using DemaWare.DemaIdentify.Enums.Setting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
[Table("Setting")]
public class Setting {
    [Key]
    public SettingType Type { get; set; }

    [MaxLength(255)]
    public string? Value { get; set; }
}
