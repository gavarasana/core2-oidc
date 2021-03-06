﻿using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using ravi.learn.idp.model;
using ravi.learn.idp.web.Services;
using ravi.learn.idp.web.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ravi.learn.idp.web.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IImageGalleryHttpClient _imageGalleryHttpClient;

        public GalleryController(IImageGalleryHttpClient imageGalleryHttpClient)
        {
            _imageGalleryHttpClient = imageGalleryHttpClient;
        }

        public async Task<IActionResult> Index()
        {
            var idToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.IdToken);
            ViewBag.Token = idToken;

            ViewBag.Claims = RetrieveClaims(User.Claims);

            // call the API
            var httpClient = await _imageGalleryHttpClient.GetClientAsync();

          //  var ownerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
           // httpClient.DefaultRequestHeaders.Add("authorization", $"bearer {User.FindFirst(ClaimTypes.NameIdentifier).Value}");
            var response = await httpClient.GetAsync($"api/images").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var imagesAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var galleryIndexViewModel = new GalleryIndexViewModel(
                    JsonConvert.DeserializeObject<IList<Image>>(imagesAsString).ToList());

                return View(galleryIndexViewModel);
            }else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Redirect("/Authorization/AccessDenied");
            }

            throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
        }

        private string RetrieveClaims(IEnumerable<Claim> claims)
        {
            var claimCollection = new StringBuilder();
            foreach (var claim in claims)
            {
                claimCollection.Append($"Claim Type: {claim.Type} Claim Value:{claim.Value}\n");
            }
            return claimCollection.ToString();
        }

        public async Task<IActionResult> EditImage(Guid id)
        {
            // call the API
            var httpClient = await _imageGalleryHttpClient.GetClientAsync();

            var response = await httpClient.GetAsync($"api/images/{id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var imageAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var deserializedImage = JsonConvert.DeserializeObject<Image>(imageAsString);

                var editImageViewModel = new EditImageViewModel()
                {
                    Id = deserializedImage.Id,
                    Title = deserializedImage.Title
                };
                
                return View(editImageViewModel);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Redirect("/Authorization/AccessDenied");
            }

            throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForUpdate instance
            var imageForUpdate = new ImageForUpdate()
                { Title = editImageViewModel.Title };

            // serialize it
            var serializedImageForUpdate = JsonConvert.SerializeObject(imageForUpdate);

            // call the API
            var httpClient = await _imageGalleryHttpClient.GetClientAsync();

            var response = await httpClient.PutAsync(
                $"api/images/{editImageViewModel.Id}",
                new StringContent(serializedImageForUpdate, System.Text.Encoding.Unicode, "application/json"))
                .ConfigureAwait(false);                        

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Redirect("/Authorization/AccessDenied");
            }
            throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            // call the API
            var httpClient = await _imageGalleryHttpClient.GetClientAsync();

            var response = await httpClient.DeleteAsync($"api/images/{id}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Redirect("/Authorization/AccessDenied");
            }
            throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
        }
        
        [Authorize(Roles = "paiduser")]
        public IActionResult AddImage()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "paiduser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
        {   
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForCreation instance
            var imageForCreation = new ImageForCreation()
                { Title = addImageViewModel.Title };

            // take the first (only) file in the Files list
            var imageFile = addImageViewModel.Files.First();

            if (imageFile.Length > 0)
            {
                using (var fileStream = imageFile.OpenReadStream())
                using (var ms = new MemoryStream())
                {
                    fileStream.CopyTo(ms);
                    imageForCreation.Bytes = ms.ToArray();                     
                }
            }
            
            // serialize it
            var serializedImageForCreation = JsonConvert.SerializeObject(imageForCreation);

            // call the API
            var httpClient = await _imageGalleryHttpClient.GetClientAsync();

            var response = await httpClient.PostAsync(
                $"api/images",
                new StringContent(serializedImageForCreation, System.Text.Encoding.Unicode, "application/json"))
                .ConfigureAwait(false); 

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Redirect("/Authorization/AccessDenied");
            }
            throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
        }               

        public async Task Logout()
        {
            var discoveryClient = new DiscoveryClient("https://localhost:44313/");
            var metaDataResponse = await discoveryClient.GetAsync();

            var revocationClient = new TokenRevocationClient(metaDataResponse.RevocationEndpoint, "ImageGallery", "secret");

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrEmpty(accessToken))
            {
                var revocationResponse = await revocationClient.RevokeAccessTokenAsync(accessToken);
                if (revocationResponse.IsError)
                {
                    throw new Exception("Problem occurred when revoking the access token", revocationResponse.Exception);
                }
            }

            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var revocationResponse = await revocationClient.RevokeAccessTokenAsync(refreshToken);
                if (revocationResponse.IsError)
                {
                    throw new Exception("Problem occurred when revoking the refresh token", revocationResponse.Exception);
                }
            }
            // will clear out the cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        //[Authorize(Roles = "paiduser")]
        [Authorize(Policy = "OrderFrame")]

        public async Task<IActionResult> OrderFrame()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);

            var discoveryClient = new DiscoveryClient("https://localhost:44313/");
            var metadataResponse = await discoveryClient.GetAsync();
            var userinfoClient = new UserInfoClient(metadataResponse.UserInfoEndpoint);
            var response = await userinfoClient.GetAsync(accessToken);
            if (response.IsError)
            {
                throw new Exception("Problem accessing the userInfo endpoint");
            }
            var address = response.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Address)?.Value;

            var orderFrameViewModel = new OrderFrameViewModel(address);
            return View(orderFrameViewModel);
        }
    }

    
}
