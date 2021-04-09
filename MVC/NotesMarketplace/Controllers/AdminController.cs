using NotesMarketplace.Models;
using NotesMarketplace.SendMail;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminController : Controller
    {
        private readonly NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Dashboard")]
        public ActionResult Dashboard(string search, string sort, string month, int page = 1)
        {

            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Month = month;

            // get memberid
            int memberid = _dbcontext.UsersRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();

            // get statusid
            int submittedforreviewid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "submitted for review").Select(x => x.ID).FirstOrDefault();
            int inreviewid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "in review").Select(x => x.ID).FirstOrDefault();

            // current date and time
            var now = DateTime.Now;
            // viewbag for monthlist
            ViewBag.MonthList = Enumerable.Range(1, 6).Select(x => new
                                                {
                                                    Value = now.AddMonths(-x + 1).ToString("MM").ToString(),
                                                    Text = now.AddMonths(-x + 1).ToString("MMMM").ToString()
                                                }).ToList();

            
            var last7days = DateTime.Now.AddDays(-7);

            // get notereview count
            var notereview = _dbcontext.SellerNotes.Where(x => (x.Status == submittedforreviewid || x.Status == inreviewid) && x.IsActive == true).Count();
            // get notes downloaded count of last 7 days
            var notedownloaded = _dbcontext.Downloads.Where(x => x.AttachmentDownloadedDate > last7days).Count();
            // get count of newly registered of last 7 days
            var newregistration = _dbcontext.Users.Where(x => x.RoleID == memberid && x.CreatedDate > last7days).Count();

            // create list object of AdminDashboardViewModel.PublishedNotesList
            var notelist = new List<AdminDashboardViewModel.PublishedNotesList>();

            // get all published notes
            var publishednotelist = _dbcontext.SellerNotes.Where(x => x.Status == 9);

            // get current month
            var currentMonth = DateTime.Now.ToString("MM");

            foreach (var item in publishednotelist)
            {
                // filter notes based on selected month
                // default we have to show current month published note
                if (String.IsNullOrEmpty(month))
                {
                    month = DateTime.Now.ToString("MM");
                    ViewBag.Month = month;
                }

                // if current month - selectd month >= 0 then don't need to subtract year by 1
                if (Convert.ToInt32(currentMonth) - Convert.ToInt32(month) >= 0)
                {
                    // get year 
                    var year = Convert.ToInt32(DateTime.Now.ToString("yyyy"));
                    // compare note's published month with 
                    bool selectedmonth = item.PublishedDate.Value.ToString("MM").Equals(month);
                    // compare note's year with selected year
                    bool selectedyear = item.PublishedDate.Value.ToString("yyyy").Equals(year.ToString());
                    // if one of them is false then we don't nedd to add notes in list
                    if (selectedmonth == false || selectedyear == false)
                    {
                        continue;
                    }
                }
                // if current month - selectd month < 0 then need to subtract year by 1
                else
                {
                    // subtract year by 1
                    var year = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1;
                    // compare note's month with selected month
                    bool selectedmonth = item.PublishedDate.Value.ToString("MM").Equals(month);
                    // compare note's year with year
                    bool selectedyear = item.PublishedDate.Value.ToString("yyyy").Equals(year.ToString());
                    // if one of them is false then we don't nedd to add notes in list
                    if (selectedmonth == false || selectedyear == false)
                    {
                        continue;
                    }
                }

                // create object of AdminDashboardViewModel.PublishedNotesList
                var note = new AdminDashboardViewModel.PublishedNotesList();

                // get attachment of notes
                var attachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == item.ID);
                
                // get download history of given note
                var downloads = _dbcontext.Downloads.Where(x => x.NoteID == item.ID && x.IsSellerHasAllowedDownload == true).Count();

                // get seller
                var publisher = _dbcontext.Users.Where(x => x.ID == item.SellerID).First();

                // declare file size var
                decimal filesize = 0;

                // iterate through each attachment
                foreach (var files in attachement)
                {
                    string filepath = Server.MapPath(files.FilePath + files.FileName);
                    FileInfo file = new FileInfo(filepath);
                    // count file size and add into filesize var
                    filesize += file.Length;
                }

                note.ID = item.ID;
                note.Title = item.Title;
                note.Category = item.NoteCategory.Name;
                note.Publisher = publisher.FirstName + " " + publisher.LastName;
                note.PublishedDate = (DateTime)item.PublishedDate;
                note.Price = item.SellingPrice;
                note.SellType = item.IsPaid == true ? "Paid" : "Free";
                note.FileSize = filesize;
                note.Downloads = downloads;

                decimal sizemb = 0, sizekb = 0;
                // get file size in mb
                if (note.FileSize / 1024 > 1024)
                {
                    sizemb = note.FileSize / (1024 * 1024);
                }
                // get file size in kb
                else
                {
                    sizekb = Math.Ceiling(note.FileSize / 1024);
                }
                // file size in kb or mb
                if(sizemb != 0)
                {
                    note.FileSizeKBMB = sizemb.ToString("0.00") + " MB";
                }
                else
                {
                    note.FileSizeKBMB = sizekb.ToString() + " KB";
                }
                // add note in notelist
                notelist.Add(note);
            }

            // if search is not empty then search from result
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                notelist = notelist.Where(x => x.Title.ToLower().Contains(search) ||
                                               x.Category.ToLower().Contains(search) ||
                                               x.FileSizeKBMB.ToLower().Contains(search) ||
                                               x.Price.ToString().Contains(search) ||
                                               x.SellType.ToLower().Contains(search) ||
                                               x.Publisher.ToLower().Contains(search) ||
                                               x.PublishedDate.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                               x.Downloads.ToString().ToString().Contains(search)
                                         ).ToList();
            }

            // create object of AdminDashboardViewModel
            AdminDashboardViewModel adminDashboardViewModel = new AdminDashboardViewModel
            {
                PublishedNotesLists = notelist.AsEnumerable(),
                NotesInReview = notereview,
                NotesDownloaded = notedownloaded,
                NewRegistration = newregistration
            };

            // sort result
            adminDashboardViewModel.PublishedNotesLists = SortTableDashboard(sort, adminDashboardViewModel.PublishedNotesLists);
            // get total pages
            ViewBag.TotalPages = Math.Ceiling(notelist.Count() / 5.0);
            // show result based on pagination
            adminDashboardViewModel.PublishedNotesLists = adminDashboardViewModel.PublishedNotesLists.Skip((page - 1) * 5).Take(5);

            return View(adminDashboardViewModel);
        }

        // sorting for dashboard published notes
        private IEnumerable<AdminDashboardViewModel.PublishedNotesList> SortTableDashboard(string sort, IEnumerable<AdminDashboardViewModel.PublishedNotesList> table)
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
                case "FileSize_Asc":
                    {
                        table = table.OrderBy(x => x.FileSize);
                        break;
                    }
                case "FileSize_Desc":
                    {
                        table = table.OrderByDescending(x => x.FileSize);
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
                case "Publisher_Asc":
                    {
                        table = table.OrderBy(x => x.Publisher);
                        break;
                    }
                case "Publisher_Desc":
                    {
                        table = table.OrderByDescending(x => x.Publisher);
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
                        table = table.OrderBy(x => x.Downloads);
                        break;
                    }
                case "Downloads_Desc":
                    {
                        table = table.OrderByDescending(x => x.Downloads);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.Downloads);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/Unpublish")]
        public ActionResult UnPublishNote(FormCollection form)
        {
            int noteid = Convert.ToInt32(form["noteid"]);
            string remark = form["unpublish-remark"];
            
            // get user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
           // get note
            var note = _dbcontext.SellerNotes.Where(x => x.ID == noteid).FirstOrDefault();

            // get note status removed id
            int removedid = _dbcontext.ReferenceDatas.Where(x => x.Value.ToLower() == "removed").Select(x => x.ID).FirstOrDefault();

            if(note == null)
            {
                return HttpNotFound();
            }

            // update note status
            note.Status = removedid;
            note.ModifiedBy = user.ID;
            note.ModifiedDate = DateTime.Now;

            // save note in database
            _dbcontext.Entry(note).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            // get seller user objecct
            var seller = _dbcontext.Users.Where(x => x.ID == note.SellerID).FirstOrDefault();

            // send mail to seller
            UnpublishNoteTemplate(remark, seller);

            return RedirectToAction("Dashboard", "Admin");
        }


        public void UnpublishNoteTemplate(string remark, User seller)
        {
            // get text from unpublishnote template from emailtemplate directory
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "UnpublishNote" + ".cshtml");
            
            // replace seller and remark
            body = body.Replace("ViewBag.SellerName", seller.FirstName);
            body = body.Replace("ViewBag.Remark", remark);
            body = body.ToString();

            // get support email
            var fromemail = _dbcontext.SystemConfigurations.Where(x => x.Name == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = seller.Email.Trim();
            subject = "Sorry! We need to remove your notes from our portal.";
            StringBuilder sb = new StringBuilder();
            sb.Append(body);
            body = sb.ToString();

            // create mailmessage object
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "NotesMarketplace");
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            // send mail (NotesMarketplace/SendMail/)
            SendingEmail.SendEmail(mail);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/{noteid}/Download")]
        public ActionResult AdminDownloadNote(int noteid)
        {
            // get note 
            var note = _dbcontext.SellerNotes.Where(x => x.ID == noteid).FirstOrDefault();
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get first object of seller note attachement for attachement
            var noteattachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == note.ID).FirstOrDefault();
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // get member role id
            int memberid = _dbcontext.UsersRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();

            // variable for attachement path
            string path;

            // check if user is admin or not
            // role id 1 = admin
            if (user.RoleID != memberid)
            {
                // get attavhement path
                path = Server.MapPath(noteattachement.FilePath);

                DirectoryInfo dir = new DirectoryInfo(path);
                // create zip of attachement
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in dir.GetFiles())
                        {
                            // file path is attachement path + file name
                            string filepath = path + item.ToString();
                            ziparchive.CreateEntryFromFile(filepath, item.ToString());
                        }
                    }
                    // return zip
                    return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Note/{noteid}")]
        public ActionResult NoteDetail(int noteid)
        {
            // get logged in user if user is logged in 
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            // get note by id
            var NoteDetail = _dbcontext.SellerNotes.Where(x => x.ID == noteid).FirstOrDefault();
            // if note is not found
            if (NoteDetail == null)
            {
                return HttpNotFound();
            }
            // get reviews and user's full name and user's image
            IEnumerable<UserReviewsViewModel> reviews = from review in _dbcontext.SellerNotesReviews
                                                        join users in _dbcontext.Users on review.ReviewedByID equals users.ID
                                                        join userprofile in _dbcontext.UserProfiles on review.ReviewedByID equals userprofile.UserID
                                                        where review.NoteID == NoteDetail.ID && review.IsActive == true
                                                        orderby review.Ratings descending
                                                        select new UserReviewsViewModel
                                                        { 
                                                          TblSellerNotesReview = review, 
                                                          TblUser = users, 
                                                          TblUserProfile = userprofile 
                                                        };
            // count reviews
            var reviewcounts = reviews.Count();
            // count average review
            decimal avgreview = 0;
            if (reviewcounts > 0)
            {
                avgreview = Math.Ceiling((from x in reviews select x.TblSellerNotesReview.Ratings).Average());
            }
            // count total spam report
            var spams = _dbcontext.SellerNotesReportedIssues.Where(x => x.NoteID == NoteDetail.ID).Count();
            // create adminnotesdetailviewmodel object
            AdminNoteDetailViewModel notesdetail = new AdminNoteDetailViewModel();
            //if user is authenticated
            if (user != null)
            {
                notesdetail.UserID = user.ID;
            }
            notesdetail.SellerNote = NoteDetail;
            notesdetail.NotesReview = reviews;
            notesdetail.AverageRating = Convert.ToInt32(avgreview);
            notesdetail.TotalReview = reviewcounts;
            notesdetail.TotalSpamReport = spams;

            return View(notesdetail);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Notes/DeleteReview/{id}")]
        public ActionResult DeleteReview(int id)
        {
            // get review object
            var review = _dbcontext.SellerNotesReviews.Where(x => x.ID == id).FirstOrDefault();

            if(review == null)
            {
                return HttpNotFound();
            }

            // remove review from database
            _dbcontext.SellerNotesReviews.Remove(review);
            _dbcontext.SaveChanges();

            return RedirectToAction("Note", new { id = review.NoteID } );
        }

    }
}