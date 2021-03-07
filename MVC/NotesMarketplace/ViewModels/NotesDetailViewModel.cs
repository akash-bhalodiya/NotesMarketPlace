using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class NotesDetailViewModel
    {
        public SellerNote sellernote { get; set; }
        public SellerNotesAttachement sellernotesattachement { get; set; }
    }
}