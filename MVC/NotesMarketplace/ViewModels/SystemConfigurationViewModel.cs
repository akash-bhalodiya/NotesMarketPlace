using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class SystemConfigurationViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        [MaxLength(100, ErrorMessage = "Email is too long")]
        public string SupportEmail { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string SupportContact { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        public string NotifyEmail { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Url]
        public string FacebookURL { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Url]
        public string TwitterURL { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Url]
        public string LinkedInURL { get; set; }

        public HttpPostedFileBase DefaultProfile { get; set; }

        public HttpPostedFileBase DefaultNote { get; set; }

        public string DefaultProfileURL { get; set; }

        public string DefaultNoteURL { get; set; }
    }
}