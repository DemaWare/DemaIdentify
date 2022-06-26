using OpenIddict.EntityFrameworkCore.Models;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, ApplicationClient, ApplicationToken> {
}
