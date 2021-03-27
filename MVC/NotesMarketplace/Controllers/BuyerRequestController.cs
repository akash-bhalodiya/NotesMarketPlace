using NotesMarketplace.SendMail;
using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    public class BuyerRequestController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();


        [Authorize]
        [Route("BuyerRequest")]
        public ActionResult BuyerRequest(string search, string sort, int page = 1)
        {
            // viewbag for active class in navigation
            ViewBag.BuyerRequest = "active";

            // viewbag for sorting, searching and pagination
            ViewBag.Sort = sort;
            ViewBag.Search = search;
            ViewBag.PageNumber = page;

            //get logged in user
            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // get buyer requests
            IEnumerable<BuyerRequestViewModel> buyerrequest = from download in _dbcontext.Downloads
                                                              join users in _dbcontext.Users on download.Downloader equals users.ID
                                                              join userprofile in _dbcontext.UserProfiles on download.Downloader equals userprofile.UserID
                                                              where download.Seller == user.ID && download.IsSellerHasAllowedDownload == false && download.AttachmentPath == null
                                                              select new BuyerRequestViewModel { TblDownload = download, TblUser = users, TblUserProfile = userprofile };

            // if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                buyerrequest = buyerrequest.Where(
                                                    x => x.TblDownload.NoteTitle.ToLower().Contains(search) ||
                                                         x.TblDownload.NoteCategory.ToLower().Contains(search) ||
                                                         x.TblUser.Email.ToLower().Contains(search) ||
                                                         x.TblDownload.PurchasedPrice.ToString().ToLower().Contains(search) ||
                                                         x.TblUserProfile.PhoneNumber.ToLower().Contains(search)
                                                 ).ToList();
            }

            // sort results
            buyerrequest = SortTableBuyerRequest(sort, buyerrequest);
            // get total pages
            ViewBag.TotalPages = Math.Ceiling(buyerrequest.Count() / 10.0);
            // get result according to pagination
            buyerrequest = buyerrequest.Skip((page - 1) * 10).Take(10);

            return View(buyerrequest);
        }

        private IEnumerable<BuyerRequestViewModel> SortTableBuyerRequest(string sort, IEnumerable<BuyerRequestViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.TblDownload.NoteTitle);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblDownload.NoteTitle);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.TblDownload.NoteCategory);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblDownload.NoteCategory);
                        break;
                    }
                case "Buyer_Asc":
                    {
                        table = table.OrderBy(x => x.TblUser.Email);
                        break;
                    }
                case "Buyer_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblUser.Email);
                        break;
                    }
                case "Phone_Asc":
                    {
                        table = table.OrderBy(x => x.TblUserProfile.PhoneNumber);
                        break;
                    }
                case "Phone_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblUserProfile.PhoneNumber);
                        break;
                    }
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.TblDownload.IsPaid);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblDownload.IsPaid);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.TblDownload.PurchasedPrice);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblDownload.PurchasedPrice);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        table = table.OrderBy(x => x.TblDownload.CreatedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.TblDownload.CreatedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.TblDownload.CreatedDate);
                        break;
                    }
            }
            return table;
        }

        [Authorize]
        [Route("BuyerRequest/AllowDownload/{id}")]
        public ActionResult AllowDownload(int id)
        {
            // get logged in user
            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            // get download object by id
            Download download = _dbcontext.Downloads.Find(id);
            // check if logged in user and note seller is same or not
            if (user.ID == download.Seller)
            {
                // get sellernoteattachement object
                SellerNotesAttachement attachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == download.NoteID && x.IsActive == true).FirstOrDefault();
                
                // update data in download table
                _dbcontext.Downloads.Attach(download);
                download.IsSellerHasAllowedDownload = true;
                download.AttachmentPath = attachement.FilePath;
                download.ModifiedBy = user.ID;
                download.ModifiedDate = DateTime.Now;
                _dbcontext.SaveChanges();

                // send mail
                AllowDownloadTemplate(download, user);

                return RedirectToAction("BuyerRequest");

            }
            else
            {
                return RedirectToAction("BuyerRequest");

            }
        }

        public void AllowDownloadTemplate(Download download, User seller)
        {
            // get text from allowdownload template from emailtemplate directory
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "SellerAllowDownloadTemplate" + ".cshtml");
            // get downloader user object
            var downloader = _dbcontext.Users.Where(x => x.ID == download.Downloader).FirstOrDefault();
            // replace seller and buyer name
            body = body.Replace("ViewBag.SellerName", seller.FirstName);
            body = body.Replace("ViewBag.BuyerName", downloader.FirstName);
            body = body.ToString();

            // get support email
            var fromemail = _dbcontext.SystemConfigurations.Where(x => x.Name == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = downloader.Email.Trim();
            subject = seller.FirstName + " Allows you to download a note";
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

    }

}