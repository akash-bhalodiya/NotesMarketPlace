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
    [RoutePrefix("BuyerRequest")]
    public class BuyerRequestController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();


        // GET: BuyerRequest
        [Authorize]
        [Route("")]
        public ActionResult Index(string search, string sortorder, string sortby, int page = 1)
        {
            ViewBag.BuyerRequest = "active";

            ViewBag.SortOrder = sortorder;
            ViewBag.Search = search;
            ViewBag.SortBy = sortby;
            ViewBag.PageNumber = page;

            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            IEnumerable<BuyerRequestViewModel> buyerrequest = from download in _dbcontext.Downloads
                                                              join users in _dbcontext.Users on download.Downloader equals users.ID
                                                              join userprofile in _dbcontext.UserProfiles on download.Downloader equals userprofile.UserID
                                                              where download.Seller == user.ID && download.IsSellerHasAllowedDownload == false
                                                              select new BuyerRequestViewModel { TblDownload = download, TblUser = users, TblUserProfile = userprofile };


            if (!string.IsNullOrEmpty(search))
            {
                buyerrequest = buyerrequest.Where(
                                                    x => x.TblDownload.NoteTitle.Contains(search) ||
                                                         x.TblDownload.NoteCategory.Contains(search) ||
                                                         x.TblUser.Email.Contains(search) ||
                                                         x.TblDownload.PurchasedPrice.ToString().Contains(search) ||
                                                         x.TblUserProfile.PhoneNumber.Contains(search)
                                                 );
            }

            buyerrequest = SortTableBuyerRequest(sortorder, sortby, buyerrequest);

            ViewBag.TotalPages = Math.Ceiling(buyerrequest.Count() / 10.0);

            buyerrequest = buyerrequest.Skip((page - 1) * 10).Take(10);

            return View(buyerrequest);
        }

        // GET : AllowDownload
        [Authorize]
        [Route("AllowDownload/{id}")]
        public ActionResult AllowDownload(int id)
        {
            User user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            Download download = _dbcontext.Downloads.Find(id);

            if (user.ID == download.Seller)
            {
                SellerNotesAttachement attachement = _dbcontext.SellerNotesAttachements.Where(x => x.NoteID == download.NoteID).FirstOrDefault();

                _dbcontext.Downloads.Attach(download);
                download.IsSellerHasAllowedDownload = true;
                download.AttachmentPath = attachement.FilePath;
                download.ModifiedBy = user.ID;
                download.ModifiedDate = DateTime.Now;
                _dbcontext.SaveChanges();

                AllowDownloadTemplate(download);

                return RedirectToAction("BuyerRequest");

            }
            else
            {
                return RedirectToAction("BuyerRequest");

            }
        }

        public void AllowDownloadTemplate(Download download)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "SellerAllowDownloadTemplate" + ".cshtml");
            var downloader = _dbcontext.Users.Where(x => x.ID == download.Downloader).FirstOrDefault();
            var seller = _dbcontext.Users.Where(x => x.ID == download.Seller).FirstOrDefault();
            body = body.Replace("@ViewBag.SellerName", seller.FirstName);
            body = body.Replace("@ViewBag.BuyerName", downloader.FirstName);
            body = body.ToString();

            string from, to, bcc, cc, subject;
            from = "supportemail";
            to = downloader.Email;
            bcc = "";
            cc = "";
            subject = seller.FirstName + " Allows you to download a note";
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
            client.Credentials = new System.Net.NetworkCredential("supportemail", "password");
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private IEnumerable<BuyerRequestViewModel> SortTableBuyerRequest(string sortorder, string sortby, IEnumerable<BuyerRequestViewModel> table)
        {
            switch (sortby)
            {
                case "Title":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblDownload.NoteTitle);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.NoteTitle);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.NoteTitle);
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
                                    table = table.OrderBy(x => x.TblDownload.NoteCategory);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.NoteCategory);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.NoteCategory);
                                    return table;
                                }
                        }
                    }

                case "Buyer":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblUser.Email);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblUser.Email);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblUser.Email);
                                    return table;
                                }
                        }
                    }

                case "Phone":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblUserProfile.PhoneNumber);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblUserProfile.PhoneNumber);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblUserProfile.PhoneNumber);
                                    return table;
                                }
                        }
                    }

                case "Type":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblDownload.IsPaid);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.IsPaid);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.IsPaid);
                                    return table;
                                }
                        }
                    }

                case "Price":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblDownload.PurchasedPrice);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.PurchasedPrice);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.PurchasedPrice);
                                    return table;
                                }
                        }
                    }

                case "DownloadedDate":
                    {
                        switch (sortorder)
                        {
                            case "Asc":
                                {
                                    table = table.OrderBy(x => x.TblDownload.CreatedDate);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.CreatedDate);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.CreatedDate);
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
                                    table = table.OrderBy(x => x.TblDownload.CreatedDate);
                                    return table;
                                }
                            case "Desc":
                                {
                                    table = table.OrderByDescending(x => x.TblDownload.CreatedDate);
                                    return table;
                                }
                            default:
                                {
                                    table = table.OrderBy(x => x.TblDownload.CreatedDate);
                                    return table;
                                }
                        }
                    }

            }
        }
    }
}