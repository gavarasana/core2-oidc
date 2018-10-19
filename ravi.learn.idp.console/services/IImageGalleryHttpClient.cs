using System.Net.Http;
using System.Threading.Tasks;

namespace ravi.learn.idp.console.services
{
    public interface IImageGalleryHttpClient
    {
        
        HttpClient GetClient();
    }
}
