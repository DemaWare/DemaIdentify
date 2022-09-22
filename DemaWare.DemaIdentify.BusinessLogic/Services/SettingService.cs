using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Enums.Setting;
using DemaWare.General.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class SettingService {
    private readonly EntitiesDbContext _entitiesContext;
    public bool IsApplicationConfigured => _entitiesContext.Settings.Any();

    #region Setting Properties
    public string ApplicationName => GetSetting(SettingType.ApplicationName).AsString() ?? "DemaIdentify";
    public string ColorBaseBackground => GetSetting(SettingType.ColorBaseBackground).AsString() ?? "#3a5fac";
    public string ColorBaseForeground => GetSetting(SettingType.ColorBaseForeground).AsString() ?? "#ffffff";
    public bool OnlyAccessBySpecifiedOrganisations => GetSetting(SettingType.OnlyAccessBySpecifiedOrganisations).AsBoolean();
    public string UrlLogoWhite => GetSetting(SettingType.UrlLogoWhite).AsString() ?? "https://static.demaidentify.nl/images/DemaIdentify_logo.750px.white.png";
    public string UrlLogoColor => GetSetting(SettingType.UrlLogoColor).AsString() ?? "https://static.demaidentify.nl/images/DemaIdentify_logo.750px.blue.png";
    public string UrlBackgroundCover => GetSetting(SettingType.UrlBackgroundCover).AsString() ?? "https://static.demaidentify.nl/images/DemaWare_Cover.light.min.jpg";
	#endregion

	public SettingService(EntitiesDbContext entitiesContext) {
        _entitiesContext = entitiesContext;
    }

    private Setting? GetSetting(SettingType settingType) {
        return _entitiesContext.Settings.Find(settingType);
    }

    public SmtpSettingModel GetSmtpSettings(string? senderAddress = null, string? senderDisplayName = null) => new() {
        Host = GetSetting(SettingType.SmtpHost).AsString() ?? "localhost",
        Port = GetSetting(SettingType.SmtpPort).AsInteger() ?? 25,
        EnableSsl = GetSetting(SettingType.SmtpEnableSsl).AsBoolean(),
        Username = GetSetting(SettingType.SmtpUsername).AsString(),
        Password = GetSetting(SettingType.SmtpPassword).AsString(),
        FromAddress = senderAddress ?? GetSetting(SettingType.SmtpFromAddress).AsString() ?? string.Empty,
        FromName = senderDisplayName ?? GetSetting(SettingType.SmtpFromName).AsString()
    };

    public IDbContextTransaction BeginTransaction() {
        return _entitiesContext.Database.BeginTransaction();
    }

    public void Save(SettingType type, object? value) {
        var setting = GetSetting(type);
        if (setting == null) {
            setting = new Setting() { Type = type };
            _entitiesContext.Settings.Add(setting);
        }

        setting.Value = value?.ToString();
        _entitiesContext.SaveChanges();
    }
}
