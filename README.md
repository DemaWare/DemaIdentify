<p align="center">
  <img src="https://user-images.githubusercontent.com/69847508/175814514-b798820a-d6e0-4160-a85b-aa1f066db534.png" style="width:500px" alt="DemaIdentify_logo 750px blue"/>
</p>

---

Default clients and scopes for Blazor clients
```
await applicationManager.CreateAsync(new ApplicationClient {
    ClientId = "DemaIdentify_Admin",
    ConsentType = ConsentTypes.Implicit,
    DisplayName = "DemaIdentify (Admin)",
    ApplicationUrl = "https://localhost:44368/",
    RedirectUris = JsonSerializer.Serialize(new string[] { "https://localhost:44368/authentication/login-callback" }),
    PostLogoutRedirectUris = JsonSerializer.Serialize(new string[] { "https://localhost:44368/authentication/logout-callback" }),
    IsVisible = false,
    Permissions = JsonSerializer.Serialize(new string[] { 
        Permissions.Endpoints.Authorization,
        Permissions.Endpoints.Logout,
        Permissions.Endpoints.Token,
        Permissions.GrantTypes.AuthorizationCode,
        Permissions.ResponseTypes.Code,

        Permissions.Scopes.Email,
        Permissions.Scopes.Profile,
        Permissions.Scopes.Roles,
        Permissions.Prefixes.Scope + "openid",
        Permissions.Prefixes.Scope + "api_demaidentify_admin",
    })
}, cancellationToken);

await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor {
    ClientId = "api://DemaIdentify_Admin",
    ClientSecret = "C655E74D-4BE3-43C0-B520-02C0D3DDDD84",
    Permissions = {
        Permissions.Endpoints.Introspection,
    }
}, cancellationToken); ;

await scopeManager.CreateAsync(new OpenIddictScopeDescriptor {
    Name = "api_demaidentify_admin",
    Resources =  {
        "api://DemaIdentify_Admin"
    }
}, cancellationToken);
```