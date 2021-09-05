using Microsoft.AspNetCore.Identity;
using OAuthServer.Data;
using OAuthServer.Models;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OAuthServer
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            ApplicationUser admin = await userManager.FindByEmailAsync("youmaomfat");
            if (!await userManager.IsInRoleAsync((admin), "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("mvc") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "mvc",
                    ClientSecret = "hi",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "narutobestanime",
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://hi/signout-callback-oidc")
                    },
                    RedirectUris =
                    {
                        new Uri("https://yourmom/signin-oidc")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "mtn_api"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
            if (await manager.FindByClientIdAsync("dinglevalley_server") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "dinglevalley_server",
                    ClientSecret = "op",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "Dingle Valley (Server)",
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://dinglevalley.net/signout-callback-oidc"), new Uri("https://www.dinglevalley.net/signout-callback-oidc"), new Uri("https://localhost:5001/signout-callback-oidc")
                    },
                    RedirectUris =
                    {
                        new Uri("https://dinglevalley.net/signin-oidc"), new Uri("https://www.dinglevalley.net/signin-oidc"), new Uri("https://localhost:5001/signin-oidc")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "dinglevalley_client_api"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
            if (await manager.FindByClientIdAsync("dinglevalley_client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "dinglevalley_client",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "Dingle Valley",
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://www.dinglevalley.net/authentication/logout-callback"), new Uri("https://dinglevalley.net/authentication/logout-callback"), new Uri("https://localhost:5001/authentication/logout-callback")
                    },
                    RedirectUris =
                    {
                        new Uri("https://www.dinglevalley.net/authentication/login-callback"), new Uri("https://dinglevalley.net/authentication/login-callback"), new Uri("https://localhost:5001/authentication/login-callback")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "dinglevalley_client_api"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
            if (await manager.FindByClientIdAsync("blog") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "blog",
                    ClientSecret = "owu",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "ww",
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://daddy/signout-callback-oidc")
                    },
                    RedirectUris =
                    {
                        new Uri("https://mommy/signin-oidc")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,

                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
            IOpenIddictScopeManager scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
            if (await scopeManager.FindByNameAsync("mtn_api") == null)
            {
                OpenIddictScopeDescriptor descriptor = new OpenIddictScopeDescriptor
                {
                    Name = "mtn_api",
                    Resources =
                        {
                            "ooollalalalla_api"
                        }
                };

                await scopeManager.CreateAsync(descriptor);
            }
            if (await scopeManager.FindByNameAsync("dinglevalley_client_api") == null)
            {
                OpenIddictScopeDescriptor descriptor = new OpenIddictScopeDescriptor
                {
                    Name = "dinglevalley_client_api",
                    Resources =
                        {
                            "dinglevalley_client_api"
                        }
                };

                await scopeManager.CreateAsync(descriptor);


            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
