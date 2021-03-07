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
using System.Web.Security;

namespace NotesMarketplace.Controllers
{
    [RoutePrefix("Account")]
    public class AccountController : Controller
    {
        readonly private NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        //GET : Account/Signup
        [HttpGet]
        [Route("Signup")]
        public ActionResult Signup()
        {
            return View();
        }

        //POST : Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Signup")]
        public ActionResult Signup(SignupViewModel signupviewmodel)
        {
            if (ModelState.IsValid)
            {
                //check if email already exists or not
                bool emailalreadyexists = _dbcontext.Users.Any(x => x.Email == signupviewmodel.Email);

                if (emailalreadyexists)
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    return View();
                }
                else 
                {
                    //create user and save into database
                    User user = new User
                    {
                        FirstName = signupviewmodel.FirstName,
                        LastName = signupviewmodel.LastName,
                        Email = signupviewmodel.Email,
                        Password = signupviewmodel.Password,
                        CreatedDate = DateTime.Now,
                        RoleID = 2,
                        IsActive = true
                    };

                    _dbcontext.Users.Add(user);
                    _dbcontext.SaveChanges();

                    //send email for verification to user
                    BuildEmailVerifyTemplate(user.ID);

                    //show success message
                    ViewBag.Success = true;

                    ModelState.Clear();

                    return View();
                }
            }
            else
            {
                return View(signupviewmodel);
            }
        }

        [Route("VerifyEmail/{regID}")]
        public ActionResult VerifyEmail(int regID)
        {
            ViewBag.regID = regID;
            return View();
        }

        //to set isemailverified true in database after verification of email
        [Route("RegisterConfirm/{regID}")]
        public ActionResult RegisterConfirm(int regID)
        {
            User user = _dbcontext.Users.FirstOrDefault(x => x.ID == regID);
            user.IsEmailVerified = true;
            _dbcontext.SaveChanges();
            return RedirectToAction("Login","Account");
        }

        //get string from email template EmailVerification.cshtml
        public void BuildEmailVerifyTemplate(int RegID)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/")+ "EmailVerification" + ".cshtml");
            var regInfo = _dbcontext.Users.FirstOrDefault(x => x.ID == RegID);
            var url = "https://localhost:44381/" + "Account/VerifyEmail?regID=" + RegID;
            body = body.Replace("@ViewBag.ConfirmationLink", url);
            body = body.Replace("@ViewBag.FirstName", regInfo.FirstName);
            body = body.ToString();
            BuildEmailVerifyTemplate(body, regInfo.Email);
        }

        //set subject, body, to, and from to MailMessege object
        public static void BuildEmailVerifyTemplate(string bodyText, string sendTo)
        {
            string from, to, bcc, cc, subject, body;
            from = "supportemail";
            to = sendTo.Trim();
            bcc = "";
            cc = "";
            subject = "Note Marketplace - Email Verification";
            StringBuilder sb = new StringBuilder();
            sb.Append(bodyText);
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

        //send mail using SmtpClient obj
        public static void SendEmail(MailMessage mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential("supportemail", "mypassword");
            try
            {
                client.Send(mail);
            }
            catch(Exception e)
            {
                Debug.WriteLine("------------- "+ e.ToString());
            }
        }

        // GET : Account/Login
        [HttpGet]
        [Route("Login")]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout");
            }
            else
            {
                return View();
            }
        }

        // POST : Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Login")]
        public ActionResult Login(LoginViewModel loginviewmodel)
        {
            if (ModelState.IsValid)
            {
                //check if user for given email address is available or not
                var user = _dbcontext.Users.FirstOrDefault(x => x.Email == loginviewmodel.Email);
                if(user != null)
                {
                    //check user is active or not
                    if (user.IsActive)
                    {
                        //check if email is verified or not
                        if (user.IsEmailVerified)
                        {
                            //check if password match or not
                            if (user.Password == loginviewmodel.Password)
                            {
                                //check if user member
                                if (user.RoleID == 2)
                                {
                                    //set authentication cookie
                                    FormsAuthentication.SetAuthCookie(user.Email, loginviewmodel.RememberMe);
                                    return RedirectToAction("Index", "Home");
                                }
                                //for user admin or superadmin
                                else
                                {
                                    //set authentication cookie
                                    FormsAuthentication.SetAuthCookie(user.Email, loginviewmodel.RememberMe);
                                    return RedirectToAction("AdminDashboard");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("Password", "Incorrect password");
                                return View(loginviewmodel);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Email", "Verify your email address");
                            return View(loginviewmodel);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Your Account with this Email is not currently active");
                        return View(loginviewmodel);
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "Incorrect email address");
                    return View(loginviewmodel);
                }
            }
            else
            {
                return View(loginviewmodel);
            }
        }

        [Authorize]
        [Route("Logout")]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // GET : Account/ForgotPassword
        [HttpGet]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST : Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotpasswordviewmodel)
        {
            if (ModelState.IsValid)
            {
                //check if given email address is available or not
                var user = _dbcontext.Users.FirstOrDefault(x => x.Email == forgotpasswordviewmodel.Email);
                
                if(user != null && user.IsActive == true)
                {
                    var temporarypassword = CreatePassword(8);
                    BuildTemporaryPasswordTemplate(user.ID, temporarypassword);
                    user.Password = temporarypassword;
                    _dbcontext.Users.Attach(user);
                    _dbcontext.Entry(user).Property(x => x.Password).IsModified = true;
                    _dbcontext.SaveChanges();
                    _dbcontext.Dispose();
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("Email", "Email address is not registered or it may be deactivated");
                    return View();
                }
            }
            else
            {
                return View(forgotpasswordviewmodel);
            }
        }

        //For creating temporary password
        public static string CreatePassword(int length)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string number = "1234567890";
            const string special = "!@#$%^&*";

            var middle = length / 2;
            StringBuilder temporarypassword = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                if (middle == length)
                {
                    temporarypassword.Append(number[rnd.Next(number.Length)]);
                }
                else if (middle - 1 == length)
                {
                    temporarypassword.Append(special[rnd.Next(special.Length)]);
                }
                else
                {
                    if (length % 2 == 0)
                    {
                        temporarypassword.Append(lower[rnd.Next(lower.Length)]);
                    }
                    else
                    {
                        temporarypassword.Append(upper[rnd.Next(upper.Length)]);
                    }
                }
            }
            return temporarypassword.ToString();
        }

        //get string from email template TemporaryPassword.cshtml
        public void BuildTemporaryPasswordTemplate(int RegID, string temporarypassword)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "TemporaryPassword" + ".cshtml");
            var regInfo = _dbcontext.Users.FirstOrDefault(x => x.ID == RegID);
            body = body.Replace("@ViewBag.TemporaryPassword", temporarypassword);
            body = body.Replace("@ViewBag.FirstName", regInfo.FirstName);
            body = body.ToString();
            BuildTemporaryPasswordTemplate(body, regInfo.Email);
        }

        //set subject, body, to, and from to MailMessege object
        public static void BuildTemporaryPasswordTemplate(string bodyText, string sendTo)
        {
            string from, to, bcc, cc, subject, body;
            from = "supportemail";
            to = sendTo.Trim();
            bcc = "";
            cc = "";
            subject = "New Temporary Password has been created for you";
            StringBuilder sb = new StringBuilder();
            sb.Append(bodyText);
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

    }
}