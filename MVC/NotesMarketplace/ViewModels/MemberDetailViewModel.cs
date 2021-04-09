using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class MemberDetailViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DOB { get; set; }
        public string PhoneNumberCountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string College { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string ProfilePicture { get; set; }
        public IEnumerable<MembersNote> Notes { get; set; }

        public class MembersNote
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string Status { get; set; }
            public int? DownloadedNotes { get; set; }
            public decimal? TotalEarning { get; set; }
            public DateTime? DateAdded { get; set; }
            public DateTime? PublishedDate { get; set; }
        }
    }
}