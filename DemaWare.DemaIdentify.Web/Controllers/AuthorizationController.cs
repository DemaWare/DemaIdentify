﻿using DemaWare.DemaIdentify.BusinessLogic.Entities;
using DemaWare.DemaIdentify.BusinessLogic.Extensions;
using DemaWare.DemaIdentify.Models.Role;
using DemaWare.DemaIdentify.Web.Helpers;
using DemaWare.DemaIdentify.Web.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DemaWare.DemaIdentify.Web.Controllers;
public class AuthorizationController : Controller {
	private readonly IOpenIddictApplicationManager _applicationManager;
	private readonly IOpenIddictAuthorizationManager _authorizationManager;
	private readonly IOpenIddictScopeManager _scopeManager;
	private readonly UserManager<User> _userManager;
	private readonly SignInManager<User> _signInManager;
	private readonly ILogger<AuthorizationController> _logger;

	public AuthorizationController(IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, IOpenIddictScopeManager scopeManager, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AuthorizationController> logger) {
		_applicationManager = applicationManager;
		_authorizationManager = authorizationManager;
		_scopeManager = scopeManager;
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	[HttpGet("~/connect/authorize")]
	[HttpPost("~/connect/authorize")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Authorize() {
		var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

		// If prompt=login was specified by the client application,
		// immediately return the user agent to the login page.
		if (request.HasPrompt(Prompts.Login)) {
			// To avoid endless login -> authorization redirects, the prompt=login flag
			// is removed from the authorization request payload before redirecting the user.
			var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

			var parameters = Request.HasFormContentType ? Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() : Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();
			parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));
			return Challenge(new AuthenticationProperties { RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters) }, IdentityConstants.ApplicationScheme);
		}

		// Retrieve the user principal stored in the authentication cookie.
		// If a max_age parameter was provided, ensure that the cookie is not too old.
		// If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
		var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
		if (result == null || !result.Succeeded || (request.MaxAge != null && result.Properties?.IssuedUtc != null && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))) {
			// If the client application requested promptless authentication,
			// return an error indicating that the user is not logged in.
			if (request.HasPrompt(Prompts.None)) return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
			return Challenge(new AuthenticationProperties { RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList()) }, IdentityConstants.ApplicationScheme);
		}

		// Retrieve the profile of the logged in user.
		var user = await _userManager.GetUserAsync(result.Principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");

		// Retrieve the application details from the database.
		if (string.IsNullOrWhiteSpace(request.ClientId)) throw new InvalidOperationException("There is no ClientID available in the current request.");
		var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

		// Retrieve the permanent authorizations associated with the user and the calling client application.
		var authorizations = await _authorizationManager.FindAsync(await _userManager.GetUserIdAsync(user), await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The ID of the calling client application cannot be found."), Statuses.Valid, AuthorizationTypes.Permanent, request.GetScopes()).ToListAsync();

		switch (await _applicationManager.GetConsentTypeAsync(application)) {
			// If the consent is external (e.g when authorizations are granted by a sysadmin),
			// immediately return an error if no authorization can be found in the database.
			case ConsentTypes.External when !authorizations.Any():
				return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

			// If the consent is implicit or if an authorization was found,
			// return an authorization response without displaying the consent form.
			case ConsentTypes.Implicit:
			case ConsentTypes.External when authorizations.Any():
			case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
				var principal = await _signInManager.CreateUserPrincipalAsync(user);

				// Note: in this sample, the granted scopes match the requested scope
				// but you may want to allow the user to uncheck specific scopes.
				// For that, simply restrict the list of scopes before calling SetScopes.
				principal.SetScopes(request.GetScopes());
				principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

				// Automatically create a permanent authorization to avoid requiring explicit consent
				// for future authorization or token requests containing the same scopes.
				var authorization = authorizations.LastOrDefault();
				authorization ??= await _authorizationManager.CreateAsync(principal, await _userManager.GetUserIdAsync(user), await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The ID of the calling client application cannot be found."), AuthorizationTypes.Permanent, principal.GetScopes());

				principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
				foreach (var claim in principal.Claims) claim.SetDestinations(GetDestinations(claim, principal));
				return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

			// At this point, no authorization was found in the database and an error must be returned
			// if the client application specified prompt=none in the authorization request.
			case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
			case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
				return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

			// In every other case, render the consent form.
			default:
				return View(new AuthorizeViewModel { ApplicationName = await _applicationManager.GetDisplayNameAsync(application), Scope = request.Scope });
		}
	}

	[Authorize, FormValueRequired("submit.Accept")]
	[HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
	public async Task<IActionResult> Accept() {
		var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

		// Retrieve the profile of the logged in user.
		var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("The user details cannot be retrieved.");

		// Retrieve the application details from the database.
		if (string.IsNullOrWhiteSpace(request.ClientId)) throw new InvalidOperationException("There is no ClientID available in the current request.");
		var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

		// Retrieve the permanent authorizations associated with the user and the calling client application.
		var authorizations = await _authorizationManager.FindAsync(await _userManager.GetUserIdAsync(user), await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The ID of the calling client application cannot be found."), Statuses.Valid, AuthorizationTypes.Permanent, request.GetScopes()).ToListAsync();

		// Note: the same check is already made in the other action but is repeated
		// here to ensure a malicious user can't abuse this POST-only endpoint and
		// force it to return a valid response without the external authorization.
		if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
			return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

		var principal = await _signInManager.CreateUserPrincipalAsync(user);

		// Note: in this sample, the granted scopes match the requested scope
		// but you may want to allow the user to uncheck specific scopes.
		// For that, simply restrict the list of scopes before calling SetScopes.
		principal.SetScopes(request.GetScopes());
		principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

		// Automatically create a permanent authorization to avoid requiring explicit consent
		// for future authorization or token requests containing the same scopes.
		var authorization = authorizations.LastOrDefault();
		authorization ??= await _authorizationManager.CreateAsync(principal, await _userManager.GetUserIdAsync(user), await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The ID of the calling client application cannot be found."), AuthorizationTypes.Permanent, principal.GetScopes());

		principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
		foreach (var claim in principal.Claims) claim.SetDestinations(GetDestinations(claim, principal));

		// Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
		return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
	}

	[Authorize, FormValueRequired("submit.Deny")]
	[HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
	// Notify OpenIddict that the authorization grant has been denied by the resource owner
	// to redirect the user agent to the client application using the appropriate response_mode.
	public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

	[HttpGet("~/connect/logout")]
	public IActionResult Logout() => View();

	[ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
	public async Task<IActionResult> LogoutPost() {
		// Ask ASP.NET Core Identity to delete the local and external cookies created
		// when the user agent is redirected from the external identity provider
		// after a successful authentication flow (e.g Google or Facebook).
		await _signInManager.SignOutAsync();

		// Returning a SignOutResult will ask OpenIddict to redirect the user agent
		// to the post_logout_redirect_uri specified by the client application or to
		// the RedirectUri specified in the authentication properties if none was set.
		return SignOut(new AuthenticationProperties { RedirectUri = "/" }, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
	}

	[HttpPost("~/connect/token"), Produces("application/json")]
	public async Task<IActionResult> Exchange() {
		var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

		if (request.IsClientCredentialsGrantType()) {
			// Note: the client credentials are automatically validated by OpenIddict:
			// if client_id or client_secret are invalid, this action won't be invoked.

			if (string.IsNullOrWhiteSpace(request.ClientId)) throw new InvalidOperationException("There is no ClientID available in the current request.");
			var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

			// Create the claims-based identity that will be used by OpenIddict to generate tokens.
			var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

			// Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
			identity.AddClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application) ?? string.Empty);
			identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application) ?? string.Empty);

			// Note: In the original OAuth 2.0 specification, the client credentials grant
			// doesn't return an identity token, which is an OpenID Connect concept.
			//
			// As a non-standardized extension, OpenIddict allows returning an id_token
			// to convey information about the client application when the "openid" scope
			// is granted (i.e specified when calling principal.SetScopes()). When the "openid"
			// scope is not explicitly set, no identity token is returned to the client application.

			var principal = new ClaimsPrincipal(identity);

			// Set the list of scopes granted to the client application in access_token.
			principal.SetScopes(request.GetScopes());
			principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
			foreach (var claim in principal.Claims) claim.SetDestinations(GetDestinations(claim, principal));

			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType()) {
			// Retrieve the claims principal stored in the authorization code/device code/refresh token.
			var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal ?? throw new InvalidOperationException("The principal cannot be retrevied from the request");

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            // Note: if you want to automatically invalidate the authorization code/refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(principal);
			if (user == null) return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

			// Ensure the user is still allowed to sign in.
			if (!await _signInManager.CanSignInAsync(user)) return Forbid(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

			foreach (var claim in principal.Claims) claim.SetDestinations(GetDestinations(claim, principal));

			// Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		throw new InvalidOperationException("The specified grant type is not supported.");
	}

	// GET: /api/userinfo
	[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
	[HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), Produces("application/json")]
	public async Task<IActionResult> Userinfo() {
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge(new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists." }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

		// Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
		var claims = new Dictionary<string, object>(StringComparer.Ordinal) {
			[Claims.Subject] = await _userManager.GetUserIdAsync(user)
		};
        if (User.HasScope(Scopes.Email)) {
			claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? throw new NullReferenceException("No email address found");
			claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
		}

		if (User.HasScope(Scopes.Phone)) {
			claims[Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user) ?? throw new NullReferenceException("No phonenumber address found");
			claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
		}

		if (User.HasScope(Scopes.Roles)) {
			claims[Claims.Role] = await _userManager.GetRolesAsync(user);
		}

		// Note: the complete list of standard claims supported by the OpenID Connect specification
		// can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
		return Ok(claims);
	}


	private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal) {
		// Note: by default, claims are NOT automatically included in the access and identity tokens.
		// To allow OpenIddict to serialize them, you must attach them a destination, that specifies
		// whether they should be included in access tokens, in identity tokens or in both.

		switch (claim.Type) {
			case Claims.Name:
				yield return Destinations.AccessToken;
				if (principal.HasScope(Scopes.Profile)) yield return Destinations.IdentityToken;
				yield break;

			case Claims.Email:
				yield return Destinations.AccessToken;
				if (principal.HasScope(Scopes.Email)) yield return Destinations.IdentityToken;
				yield break;

			case Claims.Role:
				yield return Destinations.AccessToken;
				if (principal.HasScope(Scopes.Roles)) yield return Destinations.IdentityToken;
				yield break;

			case Claims.Subject:
				yield return Destinations.AccessToken;
				yield return Destinations.IdentityToken;
				yield break;

			// Never include the security stamp in the access and identity tokens, as it's a secret value.
			case "AspNet.Identity.SecurityStamp":
				yield break;

			default:
				yield return Destinations.AccessToken;
				yield break;
		}
	}
}
