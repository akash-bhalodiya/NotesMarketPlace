using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<InProgressNote> InProgressNotes { get; set; }
        public IEnumerable<PublishedNote> PublishedNotes { get; set; }
        public int? MyDownloads { get; set; }
        public int? NumberOfSoldNotes { get; set; }
        public decimal? MoneyEarned { get; set; }
        public int? MyRejectedNotes { get; set; }
        public int? BuyerRequest { get; set; }
    }
    
    public class InProgressNote
    {
        public int NoteID { get; set; }
        public DateTime? AddedDate { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }

    public class PublishedNote
    {
        public int NoteID { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
    }

}