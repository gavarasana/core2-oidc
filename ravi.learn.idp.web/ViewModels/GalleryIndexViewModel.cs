using ravi.learn.idp.model;
using System.Collections.Generic;

namespace ravi.learn.idp.web.ViewModels
{
    public class GalleryIndexViewModel
    {
        public IEnumerable<Image> Images { get; private set; }
            = new List<Image>();

        public GalleryIndexViewModel(List<Image> images)
        {
           Images = images;
        }
    }
}
