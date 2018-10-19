
using System;
using System.Net.Http;
using System.Net.Http.Headers;



namespace ravi.learn.idp.console.services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        
        private HttpClient _httpClient = new HttpClient();
        private readonly string _accessToken;

        public ImageGalleryHttpClient(string accessToken)
        {
            _accessToken = accessToken;
        }

     

        public HttpClient GetClient()
        {
            
            _httpClient.BaseAddress = new Uri("https://localhost:44310/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.SetBearerToken(_accessToken);

            return _httpClient;
        }
    }
}

