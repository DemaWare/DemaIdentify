using DemaWare.DemaIdentify.Resources;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace DemaWare.DemaIdentify.BusinessLogic.Services; 
public class LocalizationService {
    private readonly IStringLocalizer _localizer;
    public LocalizationService(IStringLocalizerFactory localizerFactory) {
        _localizer = localizerFactory.Create(nameof(DemaIdentifyResources), typeof(DemaIdentifyResources).GetTypeInfo().Assembly.GetName().Name ?? string.Empty);
    }

    public string GetLocalizedHtmlString(string key) {
        return _localizer[key];
    }
}
