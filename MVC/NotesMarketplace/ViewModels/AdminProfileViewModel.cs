using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class AdminProfileViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid Name")]
        [MaxLength(50, ErrorMessage = "Name is too long")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid Name")]
        [MaxLength(50, ErrorMessage = "Name is too long")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        public string SecondaryEmail { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string PhoneNumberCountryCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number.")]
        public string PhoneNumber { get; set; }

        public HttpPostedFileBase ProfilePicture { get; set; }

        public string ProfilePictureUrl { get; set; }

        public List<String> CountryCodeList { get; set; }
    }
}