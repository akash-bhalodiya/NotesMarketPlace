using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminReportsController : Controller
    {
        private readonly NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("SpamReport")]
        public ActionResult SpamReport(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get spam report data for showing in table
            IEnumerable<SpamReportViewModel> reportlist = from spam in _dbcontext.SellerNotesReportedIssues
                                                          join note in _dbcontext.SellerNotes on spam.NoteID equals note.ID
                                                          join reportedby in _dbcontext.Users on spam.ReportedByID equals reportedby.ID
                                                          select new SpamReportViewModel
                                                          { 
                                                             ID = spam.ID,
                                                             NoteID = note.ID,
                                                             ReportedBy = reportedby.FirstName + " " + reportedby.LastName,
                                                             NoteTitle = note.Title,
                                                             Category = note.NoteCategory.Name,
                                                             Remark = spam.Remarks,
                                                             DateAdded = spam.CreatedDate.Value
                                                          };

            // get search result
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                reportlist = reportlist.Where(x => x.ReportedBy.ToLower().Contains(search) ||
                                                   x.NoteTitle.ToLower().Contains(search) ||
                                                   x.Category.ToLower().Contains(search) ||
                                                   x.Remark.ToLower().Contains(search) ||
                                                   x.DateAdded.ToString("dd-MM-yyyy, hh:mm").Contains(search)).ToList();
            }

            // sort result
            reportlist = SortTableReportedIssue(sort, reportlist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(reportlist.Count() / 5.0);

            // show data according to pagination
            reportlist = reportlist.Skip((page - 1) * 5).Take(5);

            return View(reportlist);
        }

        // sorting for spam report
        private IEnumerable<SpamReportViewModel> SortTableReportedIssue(string sort, IEnumerable<SpamReportViewModel> table)
        {
            switch (sort)
            {
                case "ReportedBy_Asc":
                    {
                        table = table.OrderBy(x => x.ReportedBy);
                        break;
                    }
                case "ReportedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.ReportedBy);
                        break;
                    }
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.NoteTitle);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.NoteTitle);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.Category);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.Category);
                        break;
                    }
                case "Remark_Asc":
                    {
                        table = table.OrderBy(x => x.Remark);
                        break;
                    }
                case "Remark_Desc":
                    {
                        table = table.OrderByDescending(x => x.Remark);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }

        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("SpamReport/Delete/{id}")]
        public ActionResult DeleteSpamReport(int id)
        {
            // get spam report object by id
            var spamreport = _dbcontext.SellerNotesReportedIssues.Where(x => x.ID == id).FirstOrDefault();

            // check if object is null or not
            if(spamreport == null)
            {
                return HttpNotFound();
            }

            // delete object
            _dbcontext.SellerNotesReportedIssues.Remove(spamreport);
            _dbcontext.SaveChanges();

            return RedirectToAction("SpamReport");
        }
    }
}