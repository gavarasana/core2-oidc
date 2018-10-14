using System.Net.Http;
using System.Threading.Tasks;

namespace ravi.learn.idp.web.Services
{
    public interface IImageGalleryHttpClient
    {
        Task<HttpClient> GetClientAsync();
    }
}
