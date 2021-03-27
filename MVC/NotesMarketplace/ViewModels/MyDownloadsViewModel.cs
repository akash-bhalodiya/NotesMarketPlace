using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class MyDownloadsViewModel
    {
        public Download TblDownload { get; set; }
        public User TblUser { get; set; }
        public SellerNotesReview TblReview { get; set; }
    }
}