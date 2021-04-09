using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int? NotesInReview { get; set; }
        public int? NotesDownloaded { get; set; }
        public int? NewRegistration { get; set; }
        public IEnumerable<PublishedNotesList> PublishedNotesLists { get; set; }


        public class PublishedNotesList
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string SellType { get; set; }
            public decimal? Price { get; set; }
            public string Publisher { get; set; }
            public DateTime PublishedDate { get; set; }
            public int? Downloads { get; set; }
            public decimal FileSize { get; set; }
            public string FileSizeKBMB { get; set; }
        }
    }
}