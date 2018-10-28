using IdentityModel.Client;
using ravi.learn.idp.console.services;
using System;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using ravi.learn.idp.model;
using ravi.learn.idp.console.models;
using System.Security.Principal;

namespace ravi.learn.idp.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.ReadLine();

            var task = InvokeApi();

            while (!task.IsCompleted)
            {
                Console.WriteLine("..");
            }

            var model = task.Result;
            foreach (var item in model.Images)
            {
                Console.WriteLine($"Id:{item.Id}\nTitle:{item.Title}\tFileName:{item.FileName}\n");
            }

            Console.ReadLine();
        }

        static GenericPrincipal GetGenericPrincipal()
        {

        }

        private static async Task<GalleryIndexViewModel> InvokeApi()
        {
            var errorResult = Task.FromResult(new GalleryIndexViewModel(new List<Image> { new Image { Id = Guid.Empty, Title = "Error", FileName = "Error" } })); 

            var discoveryClient = new DiscoveryClient("https://localhost:44313/");
            var metadataResponse = await discoveryClient.GetAsync();

            var tokenClient = new TokenClient(metadataResponse.TokenEndpoint, "IntegrationSvc", "-Yell0wcar-");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("System", "5y5tem", "imagegalleryapi");

            if (tokenResponse.IsError)
            {

                return await Task.FromResult(new GalleryIndexViewModel(new List<Image> { new Image { Id = Guid.Empty, Title = "Error", FileName = tokenResponse.Error } }));
            }

            var imageGalleryHttpClient = new ImageGalleryHttpClient(tokenResponse.AccessToken);
            var httpClient = imageGalleryHttpClient.GetClient();
            var response = await httpClient.GetAsync($"api/images").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var imagesAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var galleryIndexViewModel = new GalleryIndexViewModel(
                    JsonConvert.DeserializeObject<IList<Image>>(imagesAsString).ToList());

                return await Task.FromResult(galleryIndexViewModel);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return await errorResult;
            }
            return await errorResult;
        }
    }
}
