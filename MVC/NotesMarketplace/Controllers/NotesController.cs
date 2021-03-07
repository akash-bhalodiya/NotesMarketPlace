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

namespace NotesMarketplace.Controllers
{
    [RoutePrefix("Notes")]
    public class NotesController : Controller
    {

        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        // GET: Notes/AddNotes
        [HttpGet]
        [Authorize]
        [Route("AddNotes")]
        public ActionResult AddNotes()
        {
            AddNotesViewModel viewModel = new AddNotesViewModel
            {
                NoteCategoryList = _dbcontext.NoteCategories.ToList(),
                NoteTypeList = _dbcontext.NoteTypes.ToList(),
                CountryList = _dbcontext.Countries.ToList()
            };

            return View(viewModel);
        }

        // POST: Notes/AddNotes
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddNotes(AddNotesViewModel addnotesviewmodel)
        {
            if (ModelState.IsValid)
            {
                SellerNote sellernotes = new SellerNote();

                User user = _dbcontext.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

                sellernotes.SellerID = user.ID;
                sellernotes.Title = addnotesviewmodel.Title;
                sellernotes.Status = 6;
                sellernotes.Category = addnotesviewmodel.Category;
                sellernotes.NoteType = addnotesviewmodel.NoteType;
                sellernotes.NumberofPages = addnotesviewmodel.NumberofPages;
                sellernotes.Description = addnotesviewmodel.Description;
                sellernotes.UniversityName = addnotesviewmodel.UniversityName;
                sellernotes.Country = addnotesviewmodel.Country;
                sellernotes.Course = addnotesviewmodel.Course;
                sellernotes.CourseCode = addnotesviewmodel.CourseCode;
                sellernotes.Professor = addnotesviewmodel.Professor;
                sellernotes.IsPaid = addnotesviewmodel.IsPaid;
                if (sellernotes.IsPaid)
                {
                    sellernotes.SellingPrice = addnotesviewmodel.SellingPrice;
                }
                else
                {
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.CreatedDate = DateTime.Now;
                sellernotes.CreatedBy = user.ID;
                sellernotes.IsActive = true;

                _dbcontext.SellerNotes.Add(sellernotes);
                _dbcontext.SaveChanges();

                sellernotes = _dbcontext.SellerNotes.Find(sellernotes.ID);

                if (addnotesviewmodel.DisplayPicture != null)
                {
                    string displaypicturefilename = System.IO.Path.GetFileName(addnotesviewmodel.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(displaypicturepath);
                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    addnotesviewmodel.DisplayPicture.SaveAs(displaypicturefilepath);
                }

                if (addnotesviewmodel.NotesPreview != null)
                {
                    string notespreviewfilename = System.IO.Path.GetFileName(addnotesviewmodel.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(notespreviewpath);
                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    addnotesviewmodel.NotesPreview.SaveAs(notespreviewfilepath);
                }

                _dbcontext.SellerNotes.Attach(sellernotes);
                _dbcontext.Entry(sellernotes).Property(x => x.DisplayPicture).IsModified = true;
                _dbcontext.Entry(sellernotes).Property(x => x.NotesPreview).IsModified = true;
                _dbcontext.SaveChanges();

                string notesattachementfilename = System.IO.Path.GetFileName(addnotesviewmodel.UploadNotes.FileName);
                string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                CreateDirectoryIfMissing(notesattachementpath);
                string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                addnotesviewmodel.UploadNotes.SaveAs(notesattachementfilepath);

                SellerNotesAttachement notesattachements = new SellerNotesAttachement
                {
                    NoteID = sellernotes.ID,
                    FileName = notesattachementfilename,
                    FilePath = notesattachementpath + notesattachementfilename,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true
                };

                _dbcontext.SellerNotesAttachements.Add(notesattachements);
                _dbcontext.SaveChanges();
                _dbcontext.Dispose();

                return RedirectToAction("Index", "SellYourNotes");
            }
            else
            {
                AddNotesViewModel viewModel = new AddNotesViewModel
                {
                    NoteCategoryList = _dbcontext.NoteCategories.ToList(),
                    NoteTypeList = _dbcontext.NoteTypes.ToList(),
                    CountryList = _dbcontext.Countries.ToList()
                };

                return View(viewModel);
            }
        }

        private void CreateDirectoryIfMissing(string folderpath)
        {
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }

        // GET: Notes/EditNotes/id
        [HttpGet]
        [Authorize]
        [Route("EditNotes")]
        public ActionResult EditNotes(int id)
        {
            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            SellerNote note = _dbcontext.SellerNotes.Where(x => x.ID == id).FirstOrDefault();
            SellerNotesAttachement attachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == id).FirstOrDefault();
            if (user.ID == note.SellerID)
            {

                EditNotesViewModel viewModel = new EditNotesViewModel
                {
                    ID = note.ID,
                    NoteID = note.ID,
                    Title = note.Title,
                    Category = note.Category,
                    Picture = note.DisplayPicture,
                    Note = attachement.FilePath,
                    NumberofPages = note.NumberofPages,
                    Description = note.Description,
                    NoteType = note.NoteType,
                    UniversityName = note.UniversityName,
                    Course = note.Course,
                    CourseCode = note.CourseCode,
                    Country = note.Country,
                    Professor = note.Professor,
                    IsPaid = note.IsPaid,
                    SellingPrice = note.SellingPrice,
                    Preview = note.NotesPreview,
                    NoteCategoryList = _dbcontext.NoteCategories.ToList(),
                    NoteTypeList = _dbcontext.NoteTypes.ToList(),
                    CountryList = _dbcontext.Countries.ToList()
                };
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index", "SellYourNotes");
            }
        }

        // POST: Notes/EditNotes
        [HttpPost]
        [Authorize]
        [Route("EditNotes")]
        public ActionResult EditNotes(EditNotesViewModel notes)
        {
            if (ModelState.IsValid)
            {
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                var sellernotes = _dbcontext.SellerNotes.Where(x => x.ID == notes.ID).FirstOrDefault();
                var notesattachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == notes.NoteID).FirstOrDefault();

                _dbcontext.SellerNotes.Attach(sellernotes);
                sellernotes.Title = notes.Title;
                sellernotes.Category = notes.Category;
                sellernotes.NoteType = notes.NoteType;
                sellernotes.NumberofPages = notes.NumberofPages;
                sellernotes.Description = notes.Description;
                sellernotes.Country = notes.Country;
                sellernotes.UniversityName = notes.UniversityName;
                sellernotes.Course = notes.Course;
                sellernotes.CourseCode = notes.CourseCode;
                sellernotes.Professor = notes.Professor;
                if (sellernotes.IsPaid)
                {
                    sellernotes.SellingPrice = notes.SellingPrice;
                }
                else
                {
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.ModifiedDate = DateTime.Now;
                sellernotes.ModifiedBy = user.ID;

                if(notes.DisplayPicture != null)
                {
                    if(sellernotes.DisplayPicture != null)
                    {
                        string path = Server.MapPath(sellernotes.DisplayPicture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string displaypicturefilename = System.IO.Path.GetFileName(notes.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(displaypicturepath);
                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    notes.DisplayPicture.SaveAs(displaypicturefilepath);
                }                

                if (notes.NotesPreview != null)
                {
                    if (sellernotes.NotesPreview != null)
                    {
                        string path = Server.MapPath(sellernotes.NotesPreview);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string notespreviewfilename = System.IO.Path.GetFileName(notes.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(notespreviewpath);
                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    notes.NotesPreview.SaveAs(notespreviewfilepath);
                }

                if(notes.UploadNotes != null)
                {
                    if (notes.Note != null)
                    {
                        string path = Server.MapPath(notesattachement.FilePath + notesattachement.FileName);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string notesattachementfilename = System.IO.Path.GetFileName(notes.UploadNotes.FileName);
                    string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                    CreateDirectoryIfMissing(notesattachementpath);
                    string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                    notes.UploadNotes.SaveAs(notesattachementfilepath);

                    _dbcontext.SellerNotesAttachements.Attach(notesattachement);
                    notesattachement.FileName = notesattachementfilename;
                    notesattachement.FilePath = notesattachementpath;
                    notesattachement.ModifiedDate = DateTime.Now;
                    notesattachement.ModifiedBy = user.ID;
                }

                _dbcontext.SaveChanges();

                return RedirectToAction("Dashboard","Dashboard");
            }
            else
            {
                return RedirectToAction("EditNotes", new { id = notes.ID });
            }

        }

        // POST: Notes/Details/id
        [Route("Details")]
        public ActionResult Details(int id)
        {
            var NoteDetail = _dbcontext.SellerNotes.Where(x => x.ID == id).FirstOrDefault();
            var NoteAttachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == NoteDetail.ID).FirstOrDefault();
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            NotesDetailViewModel viewModel = new NotesDetailViewModel
            {
                sellernote = NoteDetail,
                sellernotesattachement = NoteAttachement
            };

            return View(viewModel);
        }

        // POST: Notes/DownloadFreeNotes/noteid/noteattachementid
        [Authorize]
        [Route("DownloadFreeNotes")]
        public ActionResult DownloadFreeNotes(int noteid,int noteattachementid)
        {
            var note = _dbcontext.SellerNotes.Find(noteid);
            var noteattachement = _dbcontext.SellerNotesAttachements.Find(noteattachementid);
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            Download download = new Download
            {
                NoteID = note.ID,
                Seller = note.SellerID,
                Downloader = user.ID,
                IsSellerHasAllowedDownload = true,
                AttachmentPath = noteattachement.FilePath,
                IsAttachmentDownloaded = true,
                AttachmentDownloadedDate = DateTime.Now,
                IsPaid = note.IsPaid,
                PurchasedPrice = note.SellingPrice,
                NoteTitle = note.Title,
                NoteCategory = note.NoteCategory.Name,
                CreatedDate = DateTime.Now,
                CreatedBy = user.ID,
            };

            _dbcontext.Downloads.Add(download);
            _dbcontext.SaveChanges();

            return File(download.AttachmentPath, "application/pdf", noteattachement.FileName);
        }

        // POST: Notes/DownloadPaidNotes/noteid/noteattachementid
        [Authorize]
        [Route("DownloadPaidNotes/{noteid}/{noteattachementid}")]
        public ActionResult DownloadPaidNotes(int noteid, int noteattachementid)
        {
            var note = _dbcontext.SellerNotes.Find(noteid);
            var noteattachement = _dbcontext.SellerNotesAttachements.Find(noteattachementid);
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            Download download = new Download
            {
                NoteID = note.ID,
                Seller = note.SellerID,
                Downloader = user.ID,
                IsSellerHasAllowedDownload = false,
                IsAttachmentDownloaded = false,
                IsPaid = note.IsPaid,
                PurchasedPrice = note.SellingPrice,
                NoteTitle = note.Title,
                NoteCategory = note.NoteCategory.Name,
                CreatedDate = DateTime.Now,
                CreatedBy = user.ID,
            };

            _dbcontext.Downloads.Add(download);
            _dbcontext.SaveChanges();
            DownloadPaidNotesTemplate(download);

            return RedirectToAction("Notes", new { id = note.ID});
        }

        public void DownloadPaidNotesTemplate(Download download)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "DownloadPaidNotes" + ".cshtml");
            var downloader = _dbcontext.Users.Where(x => x.ID == download.Downloader).FirstOrDefault();
            var seller = _dbcontext.Users.Where(x => x.ID == download.Seller).FirstOrDefault();
            body = body.Replace("@ViewBag.SellerName", seller.FirstName);
            body = body.Replace("@ViewBag.BuyerName", downloader.FirstName);
            body = body.ToString();

            string from, to, bcc, cc, subject;
            from = "email";
            to = seller.Email;
            bcc = "";
            cc = "";
            subject = downloader.FirstName + " wants to purchase your notes";
            StringBuilder sb = new StringBuilder();
            sb.Append(body);
            body = sb.ToString();
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "NotesMarketplace");
            mail.To.Add(new MailAddress(to));
            if (!string.IsNullOrEmpty(bcc))
            {
                mail.Bcc.Add(new MailAddress(bcc));
            }
            if (!string.IsNullOrEmpty(cc))
            {
                mail.CC.Add(new MailAddress(cc));
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SendEmail(mail);
        }

        public static void SendEmail(MailMessage mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential("email", "password");
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

    }
}