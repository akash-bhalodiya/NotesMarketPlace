using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class AddTypeViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid type")]
        [MaxLength(100, ErrorMessage = "Type name is too long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Description { get; set; }
    }
}