using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ravi.learn.idp.sts
{
    public static class Config
    {
        private const string ROLE_IDENTITY_RESOURCE_NAME = "roles";
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "passw0rd",
                    Claims = new List<Claim>
                    {
                        //new Claim (ClaimTypes.GivenName, "Frank"),
                        //new Claim (ClaimTypes.Surname, "Sinatra"),
                        new Claim ("given_name", "Frank"),
                        new Claim ("family_name", "Sinatra"),
                        new Claim (ClaimTypes.Gender, "Male"),
                        new Claim (ClaimTypes.DateOfBirth, "1972-09-12"),
                        new Claim("address","234 South St, Columbia, MD - 65121"),
                        new Claim("role", "freeuser")
                    }
                },
                 new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "passw0rd",
                    Claims = new List<Claim>
                    {
                        //new Claim (ClaimTypes.GivenName, "Claire"),
                        //new Claim (ClaimTypes.Surname, "Underwood"),
                        new Claim (JwtClaimTypes.GivenName, "Claire"),
                        new Claim (JwtClaimTypes.FamilyName, "Underwood"),
                        new Claim (ClaimTypes.Gender, "Female"),
                        new Claim (ClaimTypes.DateOfBirth, "1975-05-21"),
                        new Claim(JwtClaimTypes.Address,"123 Main St, Aldie, VA - 20105"),
                        new Claim(JwtClaimTypes.Role, "paiduser")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(ROLE_IDENTITY_RESOURCE_NAME,"Your role(s)", new List<string> { JwtClaimTypes.Role})
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "ImageGallery",
                    ClientName = "Image Gallery",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = new [] { "https://localhost:44360/signin-oidc" },
                    AllowedScopes = new []
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        ROLE_IDENTITY_RESOURCE_NAME
                    },
                    ClientSecrets = new[]
                    {
                        new Secret("secret".Sha256())
                    },
                    PostLogoutRedirectUris =new [] { "https://localhost:44360/signout-callback-oidc" },
                    RequireConsent = false

                }
            };
        }
    }
}
