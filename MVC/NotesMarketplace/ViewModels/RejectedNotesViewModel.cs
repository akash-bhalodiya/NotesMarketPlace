using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class RejectedNotesViewModel
    {
        public int NoteID { get; set; }
        public int SellerID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Seller { get; set; }
        public DateTime? DateEdited { get; set; }
        public string RejectedBy { get; set; }
        public string Remark { get; set; }
    }
}