using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class AdminNoteDetailViewModel
    {
        public int? UserID { get; set; }
        public SellerNote SellerNote { get; set; }
        public IEnumerable<UserReviewsViewModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }
    }

    public class UserReviewsViewModel
    {
        public User TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReview TblSellerNotesReview { get; set; }
    }

}