using DemaWare.DemaIdentify.Web.Resources;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace DemaWare.DemaIdentify.Web.Services {
    public class LocalizationService {
        private readonly IStringLocalizer _localizer;
        public LocalizationService(IStringLocalizerFactory localizerFactory) {
            _localizer = localizerFactory.Create(nameof(ModelResources), typeof(ModelResources).GetTypeInfo().Assembly.GetName().Name ?? string.Empty);
        }

        public string GetLocalizedHtmlString(string key) {
            return _localizer[key];
        }
    }
}
