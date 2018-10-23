using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ravi.learn.idp.web.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpClient> GetClientAsync()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            _httpClient.BaseAddress = new Uri("https://localhost:44310/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.SetBearerToken(accessToken);

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            var currentContext = _httpContextAccessor.HttpContext;

            // Get metadata from IDP
            var discoveryClient = new DiscoveryClient("https://localhost:44313");
            var metadataResponse = await discoveryClient.GetAsync();

            var tokenClient = new TokenClient(metadataResponse.TokenEndpoint, "ImageGallery", "secret");

            var currentRefreshToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (!tokenResult.IsError)
            {
                var updatedTokens = new List<AuthenticationToken>
                {
                    new AuthenticationToken{Name = OpenIdConnectParameterNames.IdToken, Value = tokenResult.IdentityToken},
                    new AuthenticationToken{Name = OpenIdConnectParameterNames.AccessToken, Value = tokenResult.AccessToken},
                    new AuthenticationToken{Name = OpenIdConnectParameterNames.RefreshToken, Value = tokenResult.RefreshToken}
                };

                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });

                var currentAuthenticationResult = await currentContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                currentAuthenticationResult.Properties.StoreTokens(updatedTokens);

                await currentContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentAuthenticationResult.Principal, currentAuthenticationResult.Properties);

                return tokenResult.AccessToken;

            }else
            {
                throw new Exception("Problem occured while refreshing access token", tokenResult.Exception);
            }




        }
    }
}

