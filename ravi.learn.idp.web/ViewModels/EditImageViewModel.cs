using System;
using System.ComponentModel.DataAnnotations;

namespace ravi.learn.idp.web.ViewModels
{
    public class EditImageViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public Guid Id { get; set; }  
    }
}
