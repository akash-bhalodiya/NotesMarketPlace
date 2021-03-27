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
    public class HomeController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("FAQs")]
        public ActionResult FAQs()
        {
            // viewbag for active class in navigation
            ViewBag.FAQ = "active";

            return View();
        }

        [HttpGet]
        [Route("Contactus")]
        public ActionResult Contactus()
        {
            // viewbag for active class in navigation
            ViewBag.Contactus = "active";
            // check if user is authenticated then we need to show full name and email
            if (User.Identity.IsAuthenticated) {
                // if user is authenticated then get user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                // create contact us viewmodel
                ContactusViewModel viewmodel = new ContactusViewModel();
                
                viewmodel.FullName = user.FirstName + " " + user.LastName;
                viewmodel.Email = user.Email;
                // return viewmodel
                return View(viewmodel);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Contactus")]
        public ActionResult Contactus(ContactusViewModel contactusviewmodel)
        {
            if (ModelState.IsValid)
            {
                // if user is authenticated
                if (User.Identity.IsAuthenticated)
                {
                    var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                    // if user is authenticated then email is same as user's email id that he entered during sign up
                    contactusviewmodel.Email = user.Email;
                }
                // send mail
                BuildContactusTemplate(contactusviewmodel);

                return RedirectToAction("Contactus");
            }
            else
            {
                return View(contactusviewmodel);
            }
        }

        // send mail to admin
        public void BuildContactusTemplate(ContactusViewModel contactusviewmodel)
        {
            string from, to, subject, bodytxt, body;
            // get all text from contactus from emailtemplate directory
            bodytxt = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "Contactus" + ".cshtml");
            // replace fullname and comment 
            bodytxt = bodytxt.Replace("ViewBag.FullName", contactusviewmodel.FullName);
            bodytxt = bodytxt.Replace("ViewBag.Comment", contactusviewmodel.Comment);
            bodytxt = bodytxt.ToString();

            // get support email and notify email
            var fromemail = _dbcontext.SystemConfigurations.Where(x => x.Name == "supportemail").FirstOrDefault();
            var tomail = _dbcontext.SystemConfigurations.Where(x => x.Name == "notifyemail").FirstOrDefault();

            // set from, to, subject, body
            from = fromemail.Value.Trim();
            to = tomail.Value.Trim();
            subject = contactusviewmodel.Subject + " - Query";
            StringBuilder sb = new StringBuilder();
            sb.Append(bodytxt);
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