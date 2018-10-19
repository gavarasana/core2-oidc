using ravi.learn.idp.model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ravi.learn.idp.console.models
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