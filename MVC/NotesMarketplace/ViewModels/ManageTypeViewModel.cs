using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketplace.ViewModels
{
    public class ManageTypeViewModel
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
        public string Active { get; set; }
    }
}