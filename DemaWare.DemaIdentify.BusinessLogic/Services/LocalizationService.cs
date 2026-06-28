using DemaWare.DemaIdentify.Resources;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace DemaWare.DemaIdentify.BusinessLogic.Services; 
public class LocalizationService(IStringLocalizerFactory localizerFactory) {
    private readonly IStringLocalizer _localizer = localizerFactory.Create(nameof(TextResources), typeof(TextResources).GetTypeInfo().Assembly.GetName().Name ?? string.Empty);

    public string GetLocalizedHtmlString(string key) {
        return _localizer[key];
    }
}
