using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Enums.Setting;
using DemaWare.General.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public class SettingService(EntitiesDbContext entitiesContext) {
    public bool IsApplicationConfigured => entitiesContext.Settings.Any();

	#region Setting Properties
	public string ApplicationName => GetSetting(SettingType.ApplicationName).AsString() ?? "DemaIdentify";
	public string ColorBaseBackground => GetSetting(SettingType.ColorBaseBackground).AsString() ?? "#3a5fac";
	public string ColorBaseForeground => GetSetting(SettingType.ColorBaseForeground).AsString() ?? "#ffffff";
	public bool OnlyAccessForSpecifiedOrganisations => GetSetting(SettingType.OnlyAccessForSpecifiedOrganisations).AsBoolean();
	public string UrlLogoWhite => GetSetting(SettingType.UrlLogoWhite).AsString() ?? "https://static.demaidentify.nl/images/DemaIdentify_logo.750px.white.png";
	public string UrlLogoColor => GetSetting(SettingType.UrlLogoColor).AsString() ?? "https://static.demaidentify.nl/images/DemaIdentify_logo.750px.blue.png";
	public string UrlBackgroundCover => GetSetting(SettingType.UrlBackgroundCover).AsString() ?? "https://static.demaidentify.nl/images/DemaWare_Cover.light.min.jpg";
	public bool UseDomainCredentials => GetSetting(SettingType.UseDomainCredentials).AsBoolean();

    #endregion

    private Setting? GetSetting(SettingType settingType) {
		return entitiesContext.Settings.Find(settingType);
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

	public WindowsCredentialModel GetDomainCredentials() => new() {
		ADName = GetSetting(SettingType.DomainName).AsString() ?? string.Empty,
		Username = GetSetting(SettingType.DomainUsername).AsString() ?? string.Empty,
		Password = GetSetting(SettingType.DomainPassword).AsString() ?? string.Empty
	};

	public IDbContextTransaction BeginTransaction() {
		return entitiesContext.Database.BeginTransaction();
	}

	public void Save(SettingType type, object? value) {
		var setting = GetSetting(type);
		if (setting == null) {
			setting = new Setting() { Type = type };
			entitiesContext.Settings.Add(setting);
		}

		setting.Value = value?.ToString();
		entitiesContext.SaveChanges();
	}
}
