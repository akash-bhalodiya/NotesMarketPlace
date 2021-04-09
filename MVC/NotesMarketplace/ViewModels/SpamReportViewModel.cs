using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class SpamReportViewModel
    {
        public int ID { get; set; }
        public int NoteID { get; set; }
        public string ReportedBy { get; set; }
        public string NoteTitle { get; set; }
        public string Category { get; set; }
        public DateTime DateAdded { get; set; }
        public string Remark { get; set; }
    }
}