using DemaWare.DemaIdentify.BusinessLogic;
using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.Configuration;
using DemaWare.DemaIdentify.Resources;
using DemaWare.DemaIdentify.Web;
using DemaWare.DemaIdentify.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Load config file
var openIddictConfiguration = new OpenIddictConfigurationModel();
builder.Configuration.Bind("OpenIddict", openIddictConfiguration);

// DemaIdentify
builder.Services.AddDemaIdentify(builder.Configuration.GetConnectionString(nameof(EntitiesDbContext)));

// CookiePolicy (default)
builder.Services.Configure<CookiePolicyOptions>(options => {
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.SameAsRequest;
    options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

// RequestLocalization
builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new[]     {
        new CultureInfo("en"),
        new CultureInfo("nl"),
    };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// .NET Identity
builder.Services.AddIdentity<User, Role>(options => {
    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = Claims.Role;
    options.ClaimsIdentity.EmailClaimType = Claims.Email;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = DateTime.Now.AddYears(5) - DateTime.Now;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
}).AddEntityFrameworkStores<EntitiesDbContext>().AddDefaultTokenProviders();

// OpenIddict
builder.Services.AddOpenIddict()
    .AddCore(options => {
        options.UseEntityFrameworkCore()
            .UseDbContext<EntitiesDbContext>()
            .ReplaceDefaultEntities<ApplicationClient, ApplicationAuthorization, ApplicationScope, ApplicationToken, Guid>();
    })
    .AddServer(options => {
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetLogoutEndpointUris("/connect/logout");
        options.SetIntrospectionEndpointUris("/connect/introspect");
        options.SetTokenEndpointUris("/connect/token");
        options.SetUserinfoEndpointUris("/connect/userinfo");

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OpenId);

        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AllowClientCredentialsFlow();
        options.AllowRefreshTokenFlow();


        options.UseReferenceAccessTokens();
        options.UseReferenceRefreshTokens();

        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

        if (openIddictConfiguration.UseDevelopmentCertificate) {
            options.AddDevelopmentEncryptionCertificate();
            options.AddDevelopmentSigningCertificate();
        } else {
            options.AddEncryptionCertificate(openIddictConfiguration.Certificate.Thumbprint, openIddictConfiguration.Certificate.StoreName, openIddictConfiguration.Certificate.StoreLocation);
            options.AddSigningCertificate(openIddictConfiguration.Certificate.Thumbprint, openIddictConfiguration.Certificate.StoreName, openIddictConfiguration.Certificate.StoreLocation);
        }

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();
    })
    .AddValidation(options => {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

//
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireSaRole", policy => policy.RequireRole(Constants.Roles.SystemAdministrator));
});

builder.Services.AddSession(options => {
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});

// Add initial data
builder.Services.AddHostedService<InitialData>();

//options => options.ResourcesPath = "Resources"
builder.Services.AddLocalization();

builder.Services.AddCors(options => {
    var allowedOrigins = builder.Configuration["AllowedOrigins"].Split(";");
    options.AddDefaultPolicy(x => x.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddRazorPages(options => {
    options.Conventions.AuthorizeFolder("/Admin", "RequireSaRole");
    options.Conventions.AllowAnonymousToPage("/Admin/Configure");
}).AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) => {
            return factory.Create(nameof(DemaIdentifyResources), location: typeof(DemaIdentifyResources).GetTypeInfo().Assembly.GetName().Name ?? string.Empty);
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();
app.UseCors();

var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
if (localizationOptions != null) {
    app.UseRequestLocalization(localizationOptions);
    LanguageHelper.Initialize(localizationOptions);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
//TODO: app.UseResponseCompression();
//TODO: app.UseResponseCaching();

app.UseEndpoints(endpoints => {
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

app.Run();

static void CheckSameSite(HttpContext httpContext, CookieOptions options) {
    if (options.SameSite == SameSiteMode.None) {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (DisallowsSameSiteNone(userAgent)) {
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}

/// <summary>
/// Checks if the UserAgent is known to interpret an unknown value as Strict.
/// For those the <see cref="CookieOptions.SameSite" /> property should be
/// set to <see cref="Unspecified" />.
/// </summary>
/// <remarks>
/// This code is taken from Microsoft:
/// https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
/// </remarks>
/// <param name="userAgent">The user agent string to check.</param>
/// <returns>Whether the specified user agent (browser) accepts SameSite=None or not.</returns>
static bool DisallowsSameSiteNone(string userAgent) {
    // Cover all iOS based browsers here. This includes:
    //   - Safari on iOS 12 for iPhone, iPod Touch, iPad
    //   - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
    //   - Chrome on iOS 12 for iPhone, iPod Touch, iPad
    // All of which are broken by SameSite=None, because they use the
    // iOS networking stack.
    // Notes from Thinktecture:
    // Regarding https://caniuse.com/#search=samesite iOS versions lower
    // than 12 are not supporting SameSite at all. Starting with version 13
    // unknown values are NOT treated as strict anymore. Therefore we only
    // need to check version 12.
    if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12")) {
        return true;
    }

    // Cover Mac OS X based browsers that use the Mac OS networking stack.
    // This includes:
    //   - Safari on Mac OS X.
    // This does not include:
    //   - Chrome on Mac OS X
    // because they do not use the Mac OS networking stack.
    // Notes from Thinktecture: 
    // Regarding https://caniuse.com/#search=samesite MacOS X versions lower
    // than 10.14 are not supporting SameSite at all. Starting with version
    // 10.15 unknown values are NOT treated as strict anymore. Therefore we
    // only need to check version 10.14.
    if (userAgent.Contains("Safari") && userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/")) {
        return true;
    }

    // Cover Chrome 50-69, because some versions are broken by SameSite=None
    // and none in this range require it.
    // Note: this covers some pre-Chromium Edge versions,
    // but pre-Chromium Edge does not require SameSite=None.
    // Notes from Thinktecture:
    // We can not validate this assumption, but we trust Microsofts
    // evaluation. And overall not sending a SameSite value equals to the same
    // behavior as SameSite=None for these old versions anyways.
    if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6") || userAgent.Contains("Chrome/8")) {
        return true;
    }

    return false;
}