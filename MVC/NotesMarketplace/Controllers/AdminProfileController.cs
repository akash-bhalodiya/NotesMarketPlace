using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminProfileController : Controller
    {
        private readonly NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile()
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            
            // get logged in user profile
            var userprofile = _dbcontext.UserProfiles.Where(x => x.UserID == user.ID).FirstOrDefault();

            // create AdminProfileViewModel
            AdminProfileViewModel adminProfileViewModel = new AdminProfileViewModel();
            adminProfileViewModel.FirstName = user.FirstName;
            adminProfileViewModel.LastName = user.LastName;
            adminProfileViewModel.Email = user.Email;
            adminProfileViewModel.PhoneNumberCountryCode = userprofile.PhoneNumberCountryCode;
            adminProfileViewModel.PhoneNumber = userprofile.PhoneNumber;
            
            if(userprofile.SecondaryEmailAddress != null)
            {
                adminProfileViewModel.SecondaryEmail = userprofile.SecondaryEmailAddress;
            }

            if(userprofile.ProfilePicture != null)
            {
                adminProfileViewModel.ProfilePictureUrl = userprofile.ProfilePicture;
            }

            // get country code list
            adminProfileViewModel.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();

            return View(adminProfileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile(AdminProfileViewModel obj)
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // get  logged in user profile
            var userprofile = _dbcontext.UserProfiles.Where(x => x.UserID == user.ID).FirstOrDefault();

            // check if secondary email is already exists in User or UserProfile table or not
            // if email already exists then give error
            bool secondaryemailalreadyexistsinusers = _dbcontext.Users.Where(x => x.Email == obj.SecondaryEmail).Any();
            bool secondaryemailalreadyexistsinuserprofile = _dbcontext.UserProfiles.Where(x => x.SecondaryEmailAddress == obj.Email && x.UserID != user.ID).Any();
            if (secondaryemailalreadyexistsinusers || secondaryemailalreadyexistsinuserprofile)
            {
                ModelState.AddModelError("SecondaryEmail", "This email address is already exists");
                obj.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }

            // update user's data            
            user.FirstName = obj.FirstName.Trim();
            user.LastName = obj.LastName.Trim();

            // update userprofile's data
            userprofile.SecondaryEmailAddress = obj.SecondaryEmail.Trim();
            userprofile.PhoneNumberCountryCode = obj.PhoneNumberCountryCode.Trim();
            userprofile.PhoneNumber = obj.PhoneNumber.Trim();

            // user upploaded profile picture and there is also previous profile picture then delete previous profile picture
            if (userprofile.ProfilePicture != null && obj.ProfilePicture != null)
            {
                string path = Server.MapPath(userprofile.ProfilePicture);
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
            }

            // save new profile picture and update data in userprofile table
            if (obj.ProfilePicture != null)
            {
                // get extension
                string fileextension = System.IO.Path.GetExtension(obj.ProfilePicture.FileName);
                // set new name of file
                string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                // set where to save picture
                string profilepicturepath = "~/Members/" + userprofile.UserID + "/";
                // create directory if not exists
                CreateDirectoryIfMissing(profilepicturepath);
                // get physical path and save profile picture there
                string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                obj.ProfilePicture.SaveAs(path);
                // save path in database
                userprofile.ProfilePicture = profilepicturepath + newfilename;
            }

            // update userprofile's data
            userprofile.ModifiedDate = DateTime.Now;
            userprofile.ModifiedBy = user.ID;

            // update data and save data in database
            _dbcontext.Entry(user).State = EntityState.Modified;
            _dbcontext.Entry(userprofile).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("Dashboard", "Admin");
        }

        // Create Directory
        private void CreateDirectoryIfMissing(string folderpath)
        {
            // check if directory is exists or not
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));

            // if directory is not exists then create directory
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }
    }
}