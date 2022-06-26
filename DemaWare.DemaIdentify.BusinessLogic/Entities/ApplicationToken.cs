using OpenIddict.EntityFrameworkCore.Models;

namespace DemaWare.DemaIdentify.BusinessLogic.Entities;
public class ApplicationToken : OpenIddictEntityFrameworkCoreToken<Guid, ApplicationClient, ApplicationAuthorization> {
}
