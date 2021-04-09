using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class MembersViewModel
    {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime JoiningDate { get; set; }
            public int? UnderReviewNotes { get; set; }
            public int? PublishedNotes { get; set; }
            public int? DownloadedNotes { get; set; }
            public decimal? TotalExpenses { get; set; }
            public decimal? TotalEarning { get; set; }

    }
}