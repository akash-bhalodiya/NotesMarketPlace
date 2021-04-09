using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class SignupViewModel
    {
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

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Use valid email address")]
        [DisplayName("Email *")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,24}$", ErrorMessage = "Password must be between 6 and 24 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Compare("Password", ErrorMessage = "Confirm password is not match with password")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}