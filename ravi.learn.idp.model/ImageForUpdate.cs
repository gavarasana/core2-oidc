using System.ComponentModel.DataAnnotations;

namespace ravi.learn.idp.model
{
    public class ImageForUpdate
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }      
    }
}
