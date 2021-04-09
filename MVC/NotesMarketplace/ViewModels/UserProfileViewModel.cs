using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid Name")]
        [MaxLength(50, ErrorMessage = "Name is too long")]
        [DisplayName("First Name *")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[A-Za-z ]*", ErrorMessage = "Invalid Name")]
        [MaxLength(50, ErrorMessage = "Name is too long")]
        [DisplayName("Last Name *")]
        public string LastName { get; set; }

        [DisplayName("Email *")]
        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        public string Email { get; set; }

        [DisplayName("Date of birth")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> DOB { get; set; }

        [DisplayName("Gender")]
        public Nullable<int> Gender { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string PhoneNumberCountryCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("Profile Picture")]
        public HttpPostedFileBase ProfilePicture { get; set; }

        [DisplayName("Address Line 1 *")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(100, ErrorMessage = "Address is too long")]
        public string AddressLine1 { get; set; }

        [DisplayName("Address Line 2 *")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(100, ErrorMessage = "Address is too long")]
        public string AddressLine2 { get; set; }

        [DisplayName("City *")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(50, ErrorMessage = "City name is too long")]
        public string City { get; set; }

        [DisplayName("State *")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(50, ErrorMessage = "State name is too long")]
        public string State { get; set; }

        [DisplayName("Zipcode *")]
        [Required(ErrorMessage = "This field is required")]
        public string ZipCode { get; set; }

        [DisplayName("Country *")]
        [Required(ErrorMessage = "This field is required")]
        public string Country { get; set; }

        [DisplayName("University")]
        [MaxLength(100, ErrorMessage = "University name is too long")]
        public string University { get; set; }

        [DisplayName("College")]
        [MaxLength(100, ErrorMessage = "College name is too long")]
        public string College { get; set; }
        public IEnumerable<Country> CountryList { get; set; }
        public IEnumerable<ReferenceData> GenderList { get; set; }
        public string ProfilePictureUrl { get; set; }

    }
}