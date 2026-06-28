using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.Enums.Template;
using DemaWare.DemaIdentify.Models.Template;
using DemaWare.General.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DemaWare.DemaIdentify.BusinessLogic.Services;
public partial class TemplateService(ILogger<TemplateService> logger, EntitiesDbContext entitiesContext, SettingService settingService) {
    public async Task CreateInitialTemplatesAsync() {
        if (!entitiesContext.TemplateEmails.Any()) {
            var applicationName = settingService.ApplicationName;
            var logoUrl = settingService.UrlLogoWhite;
            var baseColorBackground = settingService.ColorBaseBackground;
            var baseColorForeground = settingService.ColorBaseForeground;

            var templateParent = await entitiesContext.TemplateEmails.AddAsync(new TemplateEmail() {
                Subject = "Base template",
                Body = "<table width=\"100%\" style=\"width:100%;\" cellspacing=\"0\" cellpadding=\"0\"><tr><td width=\"100%\" align=\"center\" style=\"width:100%;\"><table width=\"600\" style=\"width:600px;font-family:Arial,Helvetica,sans-serif;\" cellspacing=\"0\" cellpadding=\"0\"><tr><td style=\"background:" + baseColorBackground + ";padding:10px;color:" + baseColorForeground + ";\" align=\"center\" colspan=\"2\"><img src=\"" + logoUrl + "\" alt=\"" + applicationName + "\" /></td></tr><tr><td style=\"padding:20px 10px;color:#555555;\" colspan=\"2\">{content}</td></tr><tr><td style=\"background:" + baseColorBackground + ";padding:10px;color:" + baseColorForeground + ";\" align=\"left\"><strong>" + applicationName + "</strong><br />Your address</td><td style=\"background:" + baseColorBackground + ";padding:10px;color:" + baseColorForeground + ";\" align=\"right\"><a href=\"tel:00000000\"style=\"color:" + baseColorForeground + "\">Your phone number</a><br /><a href=\"mailto:test@example.com\" style=\"color:" + baseColorForeground + "\";>Your email address</a><br /><a href=\"https://www.example.com\" style=\"color:" + baseColorForeground + "\">Your website</a></td></tr></table></td></tr></table>",
                IsDeleted = false
            });

            await entitiesContext.TemplateEmails.AddAsync(new TemplateEmail() {
                Parent = templateParent.Entity,
                Type = TemplateEmailType.UserRegistrationConfirmEmail,
                Subject = applicationName + " - Bevestig uw e-mailadres",
                Body = "Beste {email},<br /><br />Bedankt voor uw registratie bij " + applicationName + ". Klik hieronder om uw e-mailadres te verifieëren en uw account te activeren.<br /><br /><a href=\"{url}\">Activeer uw acccount.</a><br /><br />U wordt doorgestuurd naar de inlogpagina, waar u kunt inloggen met het e-mailadres en wachtwoord wat u tijdens uw registratie heeft opgegeven, om vervolgens uw profiel verder aan te vullen.<br /><br />Wij hopen u hiermee voldoende te hebben geinformeerd. Indien u vragen en/of opmerkingen heeft kunt u contact met ons opnemen.<br /><br />Met vriendelijke groet,<br />Uw applicatiebeheerder(s)<br />",
                IsDeleted = false
            });

            await entitiesContext.TemplateEmails.AddAsync(new TemplateEmail() {
                Parent = templateParent.Entity,
                Type = TemplateEmailType.UserResetPassword,
                Subject = applicationName + " - Uw wachtwoord vergeten?",
                Body = "Beste {email},<br /><br />U bent uw wachtwoord vergeten en u hebt ons gevraagd om een nieuw wachtwoord.<br /><br /><a href=\"{url}\">U kunt hier klikken om een nieuw wachtwoord in te stellen.</a><br /><br />Wij hopen u hiermee voldoende te hebben geinformeerd. Indien u vragen en/of opmerkingen heeft kunt u contact met ons opnemen.<br /><br />Met vriendelijke groet,<br />Uw applicatiebeheerder(s)<br />",
                IsDeleted = false
            });

            entitiesContext.SaveChanges();
            logger.LogInformation("Inserted initial email templates");
        }
    }

    #region Email
    public async Task<TemplateEmailModel> GetEmailAsync(Guid templateId) {
        if (templateId == Guid.Empty) throw new ArgumentNullException(nameof(templateId));
        var template = await entitiesContext.TemplateEmails.FirstOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted);
        if (template == null || template.IsDeleted) throw new ArgumentOutOfRangeException(nameof(templateId));
        return template.ToModel();
    }

    public async Task<IEnumerable<TemplateEmailOverviewModel>> GetEmailOverviewAsync() {
        var templateEmails = await entitiesContext.TemplateEmails.Where(x => !x.IsDeleted).ToListAsync();
        return templateEmails.Select(x => x.ToOverviewModel()).OrderBy(x => x.Type?.Name);
    }

    public async Task<IEnumerable<EnumerationModel>> GetEmailEnumerationAsync() {
        var templateEmails = await entitiesContext.TemplateEmails.Where(x => !x.IsDeleted).ToListAsync();
        return templateEmails.Select(x => x.ToEnumerationModel()).OrderBy(x => x.Name);
    }

    public async Task SaveEmailAsync(TemplateEmailModel templateModel) {
        ArgumentNullException.ThrowIfNull(templateModel);

        var template = templateModel.IsExistingObject ? await entitiesContext.TemplateEmails.FirstAsync(x => x.Id == templateModel.EntityId && !x.IsDeleted) ?? throw new ArgumentOutOfRangeException(nameof(templateModel)) : new TemplateEmail();
        if (!templateModel.IsExistingObject) entitiesContext.TemplateEmails.Add(template);
        
        template.ParentId = templateModel.Parent;
        template.Type = templateModel.Type.HasValue ? (TemplateEmailType)templateModel.Type.Value : null;
        template.Subject = !string.IsNullOrWhiteSpace(templateModel.Subject) ? templateModel.Subject : null;
        template.Body = !string.IsNullOrWhiteSpace(templateModel.Body) ? templateModel.Body : string.Empty;

        await entitiesContext.SaveChangesAsync();
        logger.LogInformation("Template ({templateSubject}) changed.", template.Subject);
    }

    public async Task DeleteEmailAsync(Guid templateId) {
        if (templateId == Guid.Empty) throw new ArgumentNullException(nameof(templateId));
        var template = await entitiesContext.TemplateEmails.FindAsync(templateId);
        if (template == null || template.IsDeleted) throw new ArgumentOutOfRangeException(nameof(templateId));

        template.IsDeleted = true;

        await entitiesContext.SaveChangesAsync();
        logger.LogInformation("Template ({templateSubject}) deleted.", template.Subject);
    }

    public TemplateEmailDetailModel GenerateEmail(TemplateEmailType emailType) {
        var templateEmail = entitiesContext.TemplateEmails.Include(x => x.Parent).FirstOrDefault(x => x.Type == emailType && !x.IsDeleted) ?? throw new IndexOutOfRangeException();
        var entityModel = templateEmail.ToDetailModel();
        entityModel.MergeTemplate();

        entityModel.Subject = ReplaceTags(entityModel.Subject);
        entityModel.Body = ReplaceTags(entityModel.Body);

        return entityModel;
    }

    private static string? ReplaceTags(string? input) {
        if (string.IsNullOrWhiteSpace(input)) return input;

        foreach (string match in FindTagsRegex().Matches(input).Cast<Match>().Select(x => x.Value).Distinct()) {
            var matchResult = match switch {
                //TODO: "{setting-webimageurl}" => entitiesContext.GetSetting(SettingType.WebImageUrl).AsString(),
                _ => match
            };

            input = input.Replace(match, matchResult);
        }
        return input;
    }

    [GeneratedRegex(@"{.*?}")]
    private static partial Regex FindTagsRegex();
    #endregion
}
