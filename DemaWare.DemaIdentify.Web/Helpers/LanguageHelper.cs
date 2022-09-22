using DemaWare.General.Models;

namespace DemaWare.DemaIdentify.Web.Helpers; 
public static class LanguageHelper {
    private static RequestLocalizationOptions? _localizationOptions;

    public static void Initialize(RequestLocalizationOptions localizationOptions) {
        _localizationOptions = localizationOptions;
    }

    public static HashSet<LanguageModel> GenerateLanguageList() {
        if (_localizationOptions != null && _localizationOptions.SupportedUICultures != null) {
            return _localizationOptions.SupportedUICultures.Select(x => new LanguageModel() {
                Code = x.TwoLetterISOLanguageName,
                Name = x.DisplayName
            }).ToHashSet();
        }
        return new HashSet<LanguageModel>();
    }
}
