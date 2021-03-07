using Microsoft.Ajax.Utilities;
using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace NotesMarketplace.Controllers
{
    [RoutePrefix("SellYourNotes")]
    public class SellYourNotesController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        // GET: SellYourNotes
        [HttpGet]
        [Authorize]
        [Route("")]
        public ActionResult Index(string inprogresssearch, string publishsearch, string sortorder, string sortby, string sortorderpublish, string sortbypublish, int inprogresspage = 1, int publishpage = 1)
        {
            ViewBag.SellYourNotes = "active";

            ViewBag.SortOrder = sortorder;
            ViewBag.SortOrderForPublishedNotes = sortorderpublish;
            ViewBag.SortBy = sortby;
            ViewBag.SortByForPublishedNotes = sortbypublish;
            ViewBag.PageNumber = inprogresspage;
            ViewBag.PageNumberPublished = publishpage;
            ViewBag.InProgress = inprogresssearch;
            ViewBag.Published = publishsearch;

            DashboardViewModel dashboardviewmodel = new DashboardViewModel();
            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            if (string.IsNullOrEmpty(inprogresssearch))
            {
                dashboardviewmodel.InProgressNotes = _dbcontext.SellerNotes.Where(x => x.SellerID == user.ID && (x.Status == 6 || x.Status == 7 || x.Status == 8));
            }
            else
            {
                dashboardviewmodel.InProgressNotes = _dbcontext.SellerNotes.Where
                                                                     (
                                                                        x => x.SellerID == user.ID &&
                                                                        (x.Status == 6 || x.Status == 7 || x.Status == 8) &&
                                                                        (x.Title.Contains(inprogresssearch) || x.NoteCategory.Name.Contains(inprogresssearch) || x.ReferenceData.Value.Contains(inprogresssearch))
                                                                     );
            }

            if (string.IsNullOrEmpty(publishsearch))
            {
                dashboardviewmodel.PublishedNotes = _dbcontext.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == 9);
            }
            else

            {
                dashboardviewmodel.PublishedNotes = _dbcontext.SellerNotes.Where
                                                    (
                                                        x => x.SellerID == user.ID &&
                                                        x.Status == 9 &&
                                                        (x.Title.Contains(publishsearch) || x.NoteCategory.Name.Contains(publishsearch) || x.SellingPrice.ToString().Contains(publishsearch))
                                                    );
            }

            dashboardviewmodel.InProgressNotes = SortTableDashboard(sortorder, sortby, dashboardviewmodel.InProgressNotes);
            dashboardviewmodel.PublishedNotes = SortTableDashboard(sortorderpublish, sortbypublish, dashboardviewmodel.PublishedNotes);

            ViewBag.TotalPagesInProgress = Math.Ceiling(dashboardviewmodel.InProgressNotes.Count() / 5.0);
            ViewBag.TotalPagesInPublished = Math.Ceiling(dashboardviewmodel.PublishedNotes.Count() / 5.0);

            dashboardviewmodel.InProgressNotes = dashboardviewmodel.InProgressNotes.Skip((inprogresspage - 1) * 5).Take(5);
            dashboardviewmodel.PublishedNotes = dashboardviewmodel.PublishedNotes.Skip((publishpage - 1) * 5).Take(5);

            return View(dashboardviewmodel);
        }

        // GET : SellYourNotes/DeleteDraft
        [Authorize]
        [Route("DeleteDraft")]
        public ActionResult DeleteDraft(int id)
        {
            SellerNote note = _dbcontext.SellerNotes.Find(id);
            SellerNotesAttachement noteattachment = _dbcontext.SellerNotesAttachements.FirstOrDefault(x => x.NoteID == id);

            string notefolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID);
            string noteattachmentfolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID + "/Attachements");

            DirectoryInfo notefolder = new DirectoryInfo(notefolderpath);
            EmptyFolder(notefolder);
            Directory.Delete(notefolderpath);

            _dbcontext.SellerNotes.Remove(note);
            _dbcontext.SellerNotesAttachements.Remove(noteattachment);
            _dbcontext.SaveChanges();

            return RedirectToAction("Index");
        }

        private IEnumerable<SellerNote> SortTableDashboard(string sortorder, string sortby, IEnumerable<SellerNote> table)
        {
            switch (sortby)
            {
                case "CreatedDate":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.CreatedDate);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.CreatedDate);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.CreatedDate);
                                    return table;
                                }
                        }
                    }

                case "Title":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.Title);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.Title);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.Title);
                                    return table;
                                }
                        }
                    }

                case "Category":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.NoteCategory.Name);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.NoteCategory.Name);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.NoteCategory.Name);
                                    return table;
                                }
                        }
                    }

                case "Status":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.ReferenceData.Value);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.ReferenceData.Value);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.ReferenceData.Value);
                                    return table;
                                }
                        }
                    }

                case "PublishedDate":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.PublishedDate);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.PublishedDate);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.PublishedDate);
                                    return table;
                                }
                        }
                    }

                case "IsPaid":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.IsPaid);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.IsPaid);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.IsPaid);
                                    return table;
                                }
                        }
                    }

                case "SellingPrice":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.SellingPrice);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.SellingPrice);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.SellingPrice);
                                    return table;
                                }
                        }
                    }

                default:
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.CreatedDate);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.CreatedDate);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.CreatedDate);
                                    return table;
                                }
                        }
                    }

            }
        }

        private void EmptyFolder(DirectoryInfo directory)
        {

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                EmptyFolder(subdirectory);
                subdirectory.Delete();
            }

        }

    }
}