using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using NotesMarketplace.SendMail;
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

        [HttpGet]
        [AllowAnonymous]
        [Route("Signup")]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
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
                    return View(signupviewmodel);
                }
                else 
                {
                    var memberid = _dbcontext.UsersRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();
                    //create user and save into database
                    User user = new User
                    {
                        FirstName = signupviewmodel.FirstName.Trim(),
                        LastName = signupviewmodel.LastName.Trim(),
                        Email = signupviewmodel.Email.Trim(),
                        Password = signupviewmodel.Password.Trim(),
                        CreatedDate = DateTime.Now,
                        RoleID = memberid,
                        IsActive = true
                    };

                    // add user in database
                    _dbcontext.Users.Add(user);
                    _dbcontext.SaveChanges();

                    // send date as string format for validate user
                    string date = user.CreatedDate.Value.ToString("ddMMyyyyHHmmss");

                    var request = HttpContext.Request;

                    string baseurl = "https://localhost:" + request.Url.Port;

                    //send email for verification to user
                    BuildEmailVerifyTemplate(baseurl,user,date);

                    //show success message
                    ViewBag.Success = true;

                    // clear modelstate
                    ModelState.Clear();

                    return View();
                }
            }
            else
            {
                return View(signupviewmodel);
            }
        }

        //get string from email template EmailVerification.cshtml
        public void BuildEmailVerifyTemplate(string baseurl, User user, string date)
        {
            // get text from email verification template
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "EmailVerification" + ".cshtml");
                        
            // create url by user id and user registered date in string format
            var url = baseurl + "/Account/VerifyEmail?key=" + user.ID + "&value=" + date;
            
            // replace url and first name
            body = body.Replace("ViewBag.ConfirmationLink", url);
            body = body.Replace("ViewBag.FirstName", user.FirstName);
            body = body.ToString();

            // get support email
            var fromemail = _dbcontext.SystemConfigurations.Where(x => x.Name == "supportemail").FirstOrDefault();
            
            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = user.Email.Trim();
            subject = "Note Marketplace - Email Verification";
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

        [AllowAnonymous]
        [Route("VerifyEmail")]
        public ActionResult VerifyEmail(int key, string value)
        {
            // viewbag for key and value
            ViewBag.Key = key;
            ViewBag.Value = value;
            return View();
        }

        //to set isemailverified true in database after verification of email
        [AllowAnonymous]
        [Route("RegisterConfirm")]
        public ActionResult RegisterConfirm(int key, string value)
        {
            // get user by key
            User user = _dbcontext.Users.Where(x => x.ID == key).FirstOrDefault();
            // if user is not found
            if(user == null)
            {
                return HttpNotFound();
            }
            // compare the date string that we get from url with user's registed date string
            if (String.Equals(user.CreatedDate.Value.ToString("ddMMyyyyHHmmss"), value))
            {
                // if both string matches then verify email
                user.IsEmailVerified = true;
                user.ModifiedBy = user.ID;
                user.ModifiedDate = DateTime.Now;
                _dbcontext.SaveChanges();
            }
            
            return RedirectToAction("Login","Account");
        }
        
        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login()
        {
            // if user is already login then make user logout
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login(LoginViewModel loginviewmodel)
        {
            // check if model state is valid or not
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
                                int memberid = _dbcontext.UsersRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();
                                //check if user member
                                if (user.RoleID == memberid)
                                {
                                    //set authentication cookie
                                    FormsAuthentication.SetAuthCookie(user.Email, loginviewmodel.RememberMe);

                                    // check if user profile exists or not
                                    var isuserprofileset = _dbcontext.UserProfiles.Where(x => x.UserID == user.ID).FirstOrDefault();

                                    // if user profile is not exists then redirect to userprofile page else search page
                                    if (isuserprofileset == null)
                                    {
                                        return RedirectToAction("UserProfile", "User");
                                    }
                                    else
                                    {
                                        return RedirectToAction("Search", "SearchNotes");
                                    }
                                }
                                //for user admin or superadmin
                                else
                                {
                                    //set authentication cookie
                                    FormsAuthentication.SetAuthCookie(user.Email, loginviewmodel.RememberMe);
                                    return RedirectToAction("Dashboard", "Admin");
                                }
                            }
                            else
                            {
                                // incorrect password error
                                ModelState.AddModelError("Password", "Incorrect password");
                                return View(loginviewmodel);
                            }
                        }
                        else
                        {
                            // email verify error
                            ModelState.AddModelError("Email", "Verify your email address");
                            return View(loginviewmodel);
                        }
                    }
                    else
                    {
                        // account is not active error
                        ModelState.AddModelError("Email", "Your Account with this Email is not currently active");
                        return View(loginviewmodel);
                    }
                }
                else
                {
                    // incorrect email address error
                    ModelState.AddModelError("Email", "Incorrect email address");
                    return View(loginviewmodel);
                }
            }
            // if model state is not valid
            else
            {
                return View(loginviewmodel);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [Route("Logout")]
        public ActionResult Logout()
        {
            // formauthentication signout
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [Route("ChangePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [Route("ChangePassword")]
        public ActionResult ChangePassword(ChangePasswordViewModel changepasswordviewmodel)
        {
            // check if model state is valid or not
            if (ModelState.IsValid)
            {
                // get logged in user
                var loggedinuser = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                // check if user is logged in or not
                if (loggedinuser != null)
                {
                    // match old password
                    if (loggedinuser.Password == changepasswordviewmodel.OldPassword)
                    {
                        // update password
                        loggedinuser.Password = changepasswordviewmodel.NewPassword;
                        loggedinuser.ModifiedDate = DateTime.Now;
                        loggedinuser.ModifiedBy = loggedinuser.ID;
                        _dbcontext.Users.Attach(loggedinuser);
                        _dbcontext.Entry(loggedinuser).Property(x => x.Password).IsModified = true;
                        _dbcontext.Entry(loggedinuser).Property(x => x.ModifiedDate).IsModified = true;
                        _dbcontext.Entry(loggedinuser).Property(x => x.ModifiedBy).IsModified = true;
                        _dbcontext.SaveChanges();

                        return RedirectToAction("login");
                    }
                    else
                    {
                        // password mismatch error
                        ModelState.AddModelError("OldPassword", "Your old password is not match with your current pasword");
                        return View(changepasswordviewmodel);
                    }
                }
                // if user is not logged in 
                else
                {
                    return RedirectToAction("Login");
                }
            }
            else
            {
                return View(changepasswordviewmodel);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotpasswordviewmodel)
        {
            if (ModelState.IsValid)
            {
                //check if given email address is available or not
                var user = _dbcontext.Users.FirstOrDefault(x => x.Email == forgotpasswordviewmodel.Email && x.IsActive == true);
                // if user object of given available address is available
                if(user != null)
                {
                    // create temporary password
                    var temporarypassword = CreatePassword(8);
                    // send temporary password to user by mail
                    BuildTemporaryPasswordTemplate(user, temporarypassword);
                    
                    // update password
                    user.Password = temporarypassword;
                    _dbcontext.Users.Attach(user);
                    _dbcontext.Entry(user).Property(x => x.Password).IsModified = true;
                    _dbcontext.SaveChanges();

                    return RedirectToAction("Login");
                }
                // if user object of given available address is not available
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
        public void BuildTemporaryPasswordTemplate(User user, string temporarypassword)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "TemporaryPassword" + ".cshtml");
                        
            body = body.Replace("ViewBag.TemporaryPassword", temporarypassword);
            body = body.Replace("ViewBag.FirstName", user.FirstName);
            body = body.ToString();

            // get support email
            var fromemail = _dbcontext.SystemConfigurations.Where(x => x.Name == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = user.Email.Trim();
            subject = "New Temporary Password has been created for you";
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