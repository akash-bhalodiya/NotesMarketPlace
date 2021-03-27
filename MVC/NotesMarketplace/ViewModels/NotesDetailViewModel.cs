using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class NotesDetailViewModel
    {
        public int? UserID { get; set; }
        public SellerNote SellerNote { get; set; }
        public IEnumerable<ReviewsViewModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }
        public bool NoteRequested { get; set; }
        public bool AllowDownload { get; set; }
    }

    public class ReviewsViewModel
    {
        public User TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReview TblSellerNotesReview { get; set; }
    }
}