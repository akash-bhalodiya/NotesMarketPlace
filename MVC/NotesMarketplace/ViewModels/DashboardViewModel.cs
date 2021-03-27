using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<SellerNote> InProgressNotes { get; set; }
        public IEnumerable<SellerNote> PublishedNotes { get; set; }
        public int? MyDownloads { get; set; }
        public int? NumberOfSoldNotes { get; set; }
        public decimal? MoneyEarned { get; set; }
        public int? MyRejectedNotes { get; set; }
        public int? BuyerRequest { get; set; }
    }
}