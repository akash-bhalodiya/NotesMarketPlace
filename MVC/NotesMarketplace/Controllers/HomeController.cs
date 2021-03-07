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
    [RoutePrefix("Home")]
    public class HomeController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        // GET: Home
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Home/FAQ
        [Route("FAQs")]
        public ActionResult FAQs()
        {
            ViewBag.FAQ = "active";

            return View();
        }

        // GET: Home/Contactus
        [Route("Contactus")]
        [HttpGet]
        public ActionResult Contactus()
        {
            ViewBag.Contactus = "active";

            return View();
        }

        // POST: Home/Contactus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contactus(ContactusViewModel contactusviewmodel)
        {
            BuildContactusTemplate(contactusviewmodel);
            ModelState.Clear();
            return View();
        }

        public static void BuildContactusTemplate(ContactusViewModel contactusviewmodel)
        {
            string from, to, bcc, cc, subject, bodytxt, body;
            bodytxt = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "Contactus" + ".cshtml");
            bodytxt = bodytxt.Replace("@ViewBag.FullName", contactusviewmodel.FullName);
            bodytxt = bodytxt.Replace("@ViewBag.Comment", contactusviewmodel.Comment);
            bodytxt = bodytxt.ToString();

            from = contactusviewmodel.Email.Trim();
            to = "email";
            bcc = "";
            cc = "";
            subject = contactusviewmodel.Subject + " - Query";
            StringBuilder sb = new StringBuilder();
            sb.Append(bodytxt);
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

        // GET : Home/SearchNotes
        [Route("SearchNotes")]
        [HttpGet]
        public ActionResult SearchNotes()
        {
            ViewBag.SearchNotes = "active";

            var resultednotes = _dbcontext.SellerNotes.Where(x => x.Status == 9).ToList();
            return View(resultednotes);
        }

    }
}