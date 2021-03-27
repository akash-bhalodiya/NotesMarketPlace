using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        // temporary
        public ContentResult Dashboard()
        {
            return Content("Admin Dashboard");
        }
    }
}