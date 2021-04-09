using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class AddCategoryViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid ategory")]
        [MaxLength(100, ErrorMessage = "Category name is too long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Description { get; set; }
    }
}