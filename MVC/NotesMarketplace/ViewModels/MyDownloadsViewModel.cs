using NotesMarketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class MyDownloadsViewModel
    {
        public int NoteID { get; set; }
        public int DownloadID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Seller { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DownloadedDate { get; set; }
        public bool NoteDownloaded {get;set;}
        public int? ReviewID { get; set; }
        public decimal? Rating { get; set; }
        public string Comment { get; set; }
    }
}