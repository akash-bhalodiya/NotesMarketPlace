using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class ContactusViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [DisplayName("Full Name *")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        [DisplayName("Email Address *")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DisplayName("Subject *")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DisplayName("Comments/Questions *")]
        public string Comment { get; set; }
    }
}