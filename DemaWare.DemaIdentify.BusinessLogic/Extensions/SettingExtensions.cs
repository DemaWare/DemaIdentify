using DemaWare.DemaIdentify.BusinessLogic.Entities;

namespace DemaWare.DemaIdentify.BusinessLogic.Extensions;
public static class SettingExtensions {
    public static bool AsBoolean(this Setting? setting) {
        return setting != null && !string.IsNullOrWhiteSpace(setting.Value) && bool.Parse(setting.Value);
    }

    public static T AsEnum<T>(this Setting? setting, T defaultValue) where T : struct {
        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enum");
        return setting != null && !string.IsNullOrWhiteSpace(setting.Value) ? (T)Enum.ToObject(typeof(T), int.Parse(setting.Value)) : defaultValue;
    }

    public static int? AsInteger(this Setting? setting) {
        return setting != null && !string.IsNullOrWhiteSpace(setting.Value) ? int.Parse(setting.Value) : null;
    }

    public static string? AsString(this Setting? setting) {
        return setting?.Value;
    }
}
