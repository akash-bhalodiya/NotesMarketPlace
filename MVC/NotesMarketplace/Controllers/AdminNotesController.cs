using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminNotesController : Controller
    {
        private readonly NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/UnderReviewNotes")]
        public ActionResult NotesUnderReview(int? member, int? seller, string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Seller = seller;
            ViewBag.Member = member;

            // get id for note status submitted for review, in review
            var submittedforreviewid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "submitted for review").Select(x => x.ID).FirstOrDefault();
            var inreviewid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "in review").Select(x => x.ID).FirstOrDefault();

            IEnumerable<NotesUnderReviewViewModel> notelist;

            // get under review notes for specific users only
            if(member != null)
            {
                ViewBag.Member = member;
                ViewBag.Seller = member;

                notelist = from notes in _dbcontext.SellerNotes
                           join sellers in _dbcontext.Users on notes.SellerID equals sellers.ID
                           where (notes.Status == submittedforreviewid || notes.Status == inreviewid) && sellers.ID == member
                           select new NotesUnderReviewViewModel
                           {
                               NoteID = notes.ID,
                               SellerID = sellers.ID,
                               Title = notes.Title,
                               Category = notes.NoteCategory.Name,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               DateAdded = notes.CreatedDate.Value,
                               Status = notes.ReferenceData.Value
                           };
            }
            // get all underreview notes
            else
            {
                notelist = from notes in _dbcontext.SellerNotes
                           join sellers in _dbcontext.Users on notes.SellerID equals sellers.ID
                           where notes.Status == submittedforreviewid || notes.Status == inreviewid
                           select new NotesUnderReviewViewModel
                           {
                               NoteID = notes.ID,
                               SellerID = sellers.ID,
                               Title = notes.Title,
                               Category = notes.NoteCategory.Name,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               DateAdded = notes.CreatedDate.Value,
                               Status = notes.ReferenceData.Value
                           };
            }
            
            // create viewbag of sellerlist
            ViewBag.SellerList = notelist.Select(x => new { 
                                                    Value = x.SellerID,
                                                    Text = x.Seller
                                                }).Distinct().ToList();

            // filter result according to selected seller
            if (seller != null)
            {
                notelist = notelist.Where(x => x.SellerID == seller).ToList();
            }

            // if search is not empty then get result based on search
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                notelist = notelist.Where(x => x.Title.ToLower().Contains(search) ||
                                               x.Category.ToLower().Contains(search) ||
                                               x.Seller.ToLower().Contains(search) ||
                                               x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                               x.Status.ToLower().Contains(search)
                                         ).ToList();
            }

            // sorting results
            notelist = SortTableNotesUnderReview(sort, notelist);

            // get totalpages
            ViewBag.TotalPages = Math.Ceiling(notelist.Count() / 5.0);

            // show result according to pagination
            notelist = notelist.Skip((page - 1) * 5).Take(5);

            return View(notelist);
        }

        // sorting for notes under reviews
        private IEnumerable<NotesUnderReviewViewModel> SortTableNotesUnderReview(string sort, IEnumerable<NotesUnderReviewViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
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
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
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
                case "Status_Asc":
                    {
                        table = table.OrderBy(x => x.Status);
                        break;
                    }
                case "Status_Desc":
                    {
                        table = table.OrderByDescending(x => x.Status);
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
        [Route("Notes/PublishedNotes")]
        public ActionResult PublishedNotes(int? member, int? seller, string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Seller = seller;
            ViewBag.Member = member;

            // get id for note status published
            var publishednoteid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "published").Select(x => x.ID).FirstOrDefault();

            IEnumerable<PublishedNoteViewModel> notelist;

            // get published notes for specific users only
            if (member != null)
            {
                ViewBag.Member = member;
                ViewBag.Seller = member;

                notelist = from notes in _dbcontext.SellerNotes
                           join sellers in _dbcontext.Users on notes.SellerID equals sellers.ID
                           join publishedby in _dbcontext.Users on notes.ActionedBy equals publishedby.ID
                           join downloads in _dbcontext.Downloads on notes.ID equals downloads.NoteID into downloadlist
                           where notes.Status == publishednoteid && notes.SellerID == member
                           select new PublishedNoteViewModel
                           {
                               NoteID = notes.ID,
                               SellerID = sellers.ID,
                               Title = notes.Title,
                               Category = notes.NoteCategory.Name,
                               SellType = notes.IsPaid == true ? "Paid" : "Free",
                               Price = notes.SellingPrice,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               PublishedDate = notes.PublishedDate.Value,
                               ApprovedBy = publishedby.FirstName + " " + publishedby.LastName,
                               NumberOfDownloads = downloadlist.Where(x => x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count()
                           };
            }
            // get all published notes
            else
            {
                notelist = from notes in _dbcontext.SellerNotes
                           join sellers in _dbcontext.Users on notes.SellerID equals sellers.ID
                           join publishedby in _dbcontext.Users on notes.ActionedBy equals publishedby.ID
                           join downloads in _dbcontext.Downloads on notes.ID equals downloads.NoteID into downloadlist
                           where notes.Status == publishednoteid
                           select new PublishedNoteViewModel
                           {
                               NoteID = notes.ID,
                               SellerID = sellers.ID,
                               Title = notes.Title,
                               Category = notes.NoteCategory.Name,
                               SellType = notes.IsPaid == true ? "Paid" : "Free",
                               Price = notes.SellingPrice,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               PublishedDate = notes.PublishedDate.Value,
                               ApprovedBy = publishedby.FirstName + " " + publishedby.LastName,
                               NumberOfDownloads = downloadlist.Where(x => x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count()
                           };
            }

            // get seller list
            ViewBag.SellerList = notelist.Select(x => new {
                                                        Value = x.SellerID,
                                                        Text = x.Seller
                                                }).Distinct().ToList();
            
            // filter result based on selected seller
            if (seller != null)
            {
                notelist = notelist.Where(x => x.SellerID == seller).ToList();
            }

            // if search is not empty then get result based on search
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                notelist = notelist.Where(x => x.Title.ToLower().Contains(search) ||
                                               x.Category.ToLower().Contains(search) ||
                                               x.SellType.ToLower().Contains(search) ||
                                               x.Price.ToString().Contains(search) ||
                                               x.Seller.ToLower().Contains(search) ||
                                               x.ApprovedBy.ToLower().Contains(search) ||
                                               x.PublishedDate.ToString("dd-MM-yyyy hh:mm").Contains(search)
                                         ).ToList();
            }

            // sort result
            notelist = SortTablePublishedNotes(sort, notelist);

            // viewbag for count total pages
            ViewBag.TotalPages = Math.Ceiling(notelist.Count() / 5.0);

            // show result according to pagination
            notelist = notelist.Skip((page - 1) * 5).Take(5);

            return View(notelist);
        }

        // sorting for published notes
        private IEnumerable<PublishedNoteViewModel> SortTablePublishedNotes(string sort, IEnumerable<PublishedNoteViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
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
                case "SellType_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "SellType_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
                        break;
                    }
                case "ApprovedBy_Asc":
                    {
                        table = table.OrderBy(x => x.ApprovedBy);
                        break;
                    }
                case "ApprovedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.ApprovedBy);
                        break;
                    }
                case "PublishedDate_Asc":
                    {
                        table = table.OrderBy(x => x.PublishedDate);
                        break;
                    }
                case "PublishedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
                case "Downloads_Asc":
                    {
                        table = table.OrderBy(x => x.NumberOfDownloads);
                        break;
                    }
                case "Downloads_Desc":
                    {
                        table = table.OrderByDescending(x => x.NumberOfDownloads);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/DownloadedNotes")]
        public ActionResult DownloadedNotes(int? selectednote, int? member, int? note, int? seller, int? buyer, string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Seller = seller;
            ViewBag.Buyer = buyer;
            ViewBag.Note = note;

            IEnumerable<DownloadedNotesViewModel> notelist;

            // get downloaded notes for specific notes only
            if(selectednote != null)
            {
                ViewBag.SelectedNote = selectednote;
                ViewBag.Note = selectednote;

                notelist = from downloads in _dbcontext.Downloads
                           join sellers in _dbcontext.Users on downloads.Seller equals sellers.ID
                           join buyers in _dbcontext.Users on downloads.Downloader equals buyers.ID
                           where downloads.IsSellerHasAllowedDownload == true && downloads.AttachmentPath != null && downloads.NoteID == selectednote
                           select new DownloadedNotesViewModel
                           {
                               NoteID = downloads.NoteID,
                               SellerID = sellers.ID,
                               BuyerID = buyers.ID,
                               Title = downloads.NoteTitle,
                               Category = downloads.NoteCategory,
                               Buyer = buyers.FirstName + " " + buyers.LastName,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               SellType = downloads.IsPaid == true ? "Paid" : "Free",
                               Price = downloads.PurchasedPrice,
                               DownloadedDate = downloads.CreatedDate.Value
                           };
            }
            // get downloaded notes for specific member only
            else if (member != null)
            {
                ViewBag.Member = member;
                ViewBag.Buyer = member;

                notelist = from downloads in _dbcontext.Downloads
                           join sellers in _dbcontext.Users on downloads.Seller equals sellers.ID
                           join buyers in _dbcontext.Users on downloads.Downloader equals buyers.ID
                           where downloads.IsSellerHasAllowedDownload == true && downloads.AttachmentPath != null && downloads.Downloader == member
                           select new DownloadedNotesViewModel
                           {
                               NoteID = downloads.NoteID,
                               SellerID = sellers.ID,
                               BuyerID = buyers.ID,
                               Title = downloads.NoteTitle,
                               Category = downloads.NoteCategory,
                               Buyer = buyers.FirstName + " " + buyers.LastName,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               SellType = downloads.IsPaid == true ? "Paid" : "Free",
                               Price = downloads.PurchasedPrice,
                               DownloadedDate = downloads.CreatedDate.Value
                           };
            }
            // get all downloaded notes
            else
            {
                notelist = from downloads in _dbcontext.Downloads
                           join sellers in _dbcontext.Users on downloads.Seller equals sellers.ID
                           join buyers in _dbcontext.Users on downloads.Downloader equals buyers.ID
                           where downloads.IsSellerHasAllowedDownload == true && downloads.AttachmentPath != null
                           select new DownloadedNotesViewModel
                           {
                               NoteID = downloads.NoteID,
                               SellerID = sellers.ID,
                               BuyerID = buyers.ID,
                               Title = downloads.NoteTitle,
                               Category = downloads.NoteCategory,
                               Buyer = buyers.FirstName + " " + buyers.LastName,
                               Seller = sellers.FirstName + " " + sellers.LastName,
                               SellType = downloads.IsPaid == true ? "Paid" : "Free",
                               Price = downloads.PurchasedPrice,
                               DownloadedDate = downloads.CreatedDate.Value
                           };
            }

            // get sellerlist
            ViewBag.SellerList = notelist.Select(x => new {
                                                    Value = x.SellerID,
                                                    Text = x.Seller
                                                }).Distinct().ToList();

            // get buyerlist
            ViewBag.BuyerList = notelist.Select(x => new {
                                                    Value = x.BuyerID,
                                                    Text = x.Buyer
                                                }).Distinct().ToList();

            // get notelist
            ViewBag.NoteList = notelist.Select(x => new {
                                                    Value = x.NoteID,
                                                    Text = x.Title
                                                }).Distinct().ToList();

            // filter result based on seller
            if (seller != null)
            {
                notelist = notelist.Where(x => x.SellerID == seller).ToList();
            }
            // filter result based on buyer
            if (buyer != null)
            {
                notelist = notelist.Where(x => x.BuyerID == buyer).ToList();
            }
            // filter result based on note
            if (note != null)
            {
                notelist = notelist.Where(x => x.NoteID == note).ToList();
            }

            // if search is not empty then get result based on search
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                notelist = notelist.Where(x => x.Title.ToLower().Contains(search) ||
                                               x.Category.ToLower().Contains(search) ||
                                               x.Buyer.ToLower().Contains(search) ||
                                               x.Seller.ToLower().Contains(search) ||
                                               x.Price.ToString().Contains(search) ||
                                               x.SellType.ToString().Contains(search) ||
                                               x.DownloadedDate.ToString("dd-MM-yyyy hh:mm").Contains(search)
                                         ).ToList();
            }

            // sort result
            notelist = SortTableDownloadedNotes(sort, notelist);

            // gte totalpages
            ViewBag.TotalPages = Math.Ceiling(notelist.Count() / 5.0);

            // show result according to pagination
            notelist = notelist.Skip((page - 1) * 5).Take(5);

            return View(notelist);
        }

        // sorting for downloaded notes
        private IEnumerable<DownloadedNotesViewModel> SortTableDownloadedNotes(string sort, IEnumerable<DownloadedNotesViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
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
                case "Buyer_Asc":
                    {
                        table = table.OrderBy(x => x.Buyer);
                        break;
                    }
                case "Buyer_Desc":
                    {
                        table = table.OrderByDescending(x => x.Buyer);
                        break;
                    }
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
                        break;
                    }
                case "SellType_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "SellType_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/RejectedNotes")]
        public ActionResult RejectedNotes(int? seller, string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Seller = seller;

            // get id for note status rejected
            var rejectedid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "rejected").Select(x => x.ID).FirstOrDefault();

            // get all rejected notes
            IEnumerable<RejectedNotesViewModel> notelist = from notes in _dbcontext.SellerNotes
                                                           join sellers in _dbcontext.Users on notes.SellerID equals sellers.ID
                                                           join rejectedby in _dbcontext.Users on notes.ActionedBy equals rejectedby.ID
                                                           where notes.Status == rejectedid
                                                           select new RejectedNotesViewModel
                                                           {
                                                               NoteID = notes.ID,
                                                               SellerID = sellers.ID,
                                                               Title = notes.Title,
                                                               Category = notes.NoteCategory.Name,
                                                               Seller = sellers.FirstName + " " + sellers.LastName,
                                                               DateEdited = notes.ModifiedDate.Value,
                                                               RejectedBy = rejectedby.FirstName + " " + rejectedby.LastName,
                                                               Remark = notes.AdminRemarks
                                                           };
            // get seller list
            ViewBag.SellerList = notelist.Select(x => new {
                                                    Value = x.SellerID,
                                                    Text = x.Seller
                                                }).Distinct().ToList();
            
            // get result for selected seller
            if (seller != null)
            {
                notelist = notelist.Where(x => x.SellerID == seller).ToList();
            }

            // if search is not empty then get result based on search
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                notelist = notelist.Where(x => x.Title.ToLower().Contains(search) ||
                                               x.Category.ToLower().Contains(search) ||
                                               x.Seller.ToLower().Contains(search) ||
                                               x.RejectedBy.ToLower().Contains(search) ||
                                               x.Remark.ToLower().Contains(search) ||
                                               x.DateEdited.Value.ToString("dd-MM-yyyy, hh:mm").Contains(search)
                                         ).ToList();
            }

            // sort result
            notelist = SortTableRejectedNotes(sort, notelist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(notelist.Count() / 5.0);

            // show result according to pagination
            notelist = notelist.Skip((page - 1) * 5).Take(5);

            return View(notelist);
        }

        // sorting for rejected notes
        private IEnumerable<RejectedNotesViewModel> SortTableRejectedNotes(string sort, IEnumerable<RejectedNotesViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
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
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
                        break;
                    }
                case "DateEdited_Asc":
                    {
                        table = table.OrderBy(x => x.DateEdited);
                        break;
                    }
                case "DateEdited_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateEdited);
                        break;
                    }
                case "RejectedBy_Asc":
                    {
                        table = table.OrderBy(x => x.RejectedBy);
                        break;
                    }
                case "RejectedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.RejectedBy);
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
                default:
                    {
                        table = table.OrderByDescending(x => x.DateEdited);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/UnderReviewNotes/InReview/{id}")]
        public ActionResult MakeNoteInReview(int id)
        {
            // get note by id
            var note = _dbcontext.SellerNotes.Where(x => x.ID == id).FirstOrDefault();

            if(note == null)
            {
                return HttpNotFound();
            }

            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // notes status id for in review
            var inreviewid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "in review").Select(x => x.ID).FirstOrDefault();

            // update status
            note.Status = inreviewid;
            note.ModifiedDate = DateTime.Now;
            note.ModifiedBy = user.ID;

            // save note in database
            _dbcontext.Entry(note).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("NotesUnderReview");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/UnderReviewNotes/Rejected")]
        public ActionResult MakeNoteReject(FormCollection form)
        {
            // get noteid
            int noteid = Convert.ToInt32(form["noteid"]);
            // get remark
            string remark = form["remark"];

            // get note by id
            var note = _dbcontext.SellerNotes.Where(x => x.ID == noteid).FirstOrDefault();

            if(note == null)
            {
                return HttpNotFound();
            }

            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).First();

            // id for note status rejected
            var rejectedid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "rejected").Select(x => x.ID).FirstOrDefault();

            // update note status
            note.Status = rejectedid;
            note.AdminRemarks = remark;
            note.ActionedBy = user.ID;
            note.ModifiedDate = DateTime.Now;
            note.ModifiedBy = user.ID;

            // save note in database
            _dbcontext.Entry(note).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("NotesUnderReview");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/UnderReviewNotes/Publish/{id}")]
        public ActionResult MakeNotePublish(int id)
        {
            // get note
            var note = _dbcontext.SellerNotes.Where(x => x.ID == id).FirstOrDefault();
            
            if(note == null)
            {
                return HttpNotFound();
            }
            
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // id for note status published
            var publishednoteid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "published").Select(x => x.ID).FirstOrDefault();

            // update note status
            note.Status = publishednoteid;
            note.PublishedDate = DateTime.Now;
            note.ActionedBy = user.ID;
            note.ModifiedDate = DateTime.Now;
            note.ModifiedBy = user.ID;

            // save data in database
            _dbcontext.Entry(note).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("Dashboard", "Admin");
        }

    }
}