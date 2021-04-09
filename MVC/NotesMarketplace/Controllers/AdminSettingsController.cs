using NotesMarketplace.Models;
using NotesMarketplace.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly NotesMarketplaceEntities _dbcontext = new NotesMarketplaceEntities();

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCategory/Add")]
        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCategory/Add")]
        public ActionResult AddCategory(AddCategoryViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // create object of NoteCategory
                NoteCategory noteCategory = new NoteCategory
                {
                    Name = obj.Name.Trim(),
                    Description = obj.Description.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true
                };

                // add category in database and save
                _dbcontext.NoteCategories.Add(noteCategory);
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageCategory");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCategory/Edit/{id}")]
        public ActionResult EditCategory(int id)
        {
            // get category
            var category = _dbcontext.NoteCategories.Where(x => x.ID == id).FirstOrDefault();

            // check if category is null or not
            if(category == null)
            {
                return HttpNotFound();
            }

            // create object of addcategoryviewmodel 
            AddCategoryViewModel editCategory = new AddCategoryViewModel
            {
                ID = category.ID,
                Name = category.Name,
                Description = category.Description
            };

            return View(editCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCategory/Edit/{id}")]
        public ActionResult EditCategory(AddCategoryViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // get category object
                var category = _dbcontext.NoteCategories.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if category is null or not
                if (category == null)
                {
                    return HttpNotFound();
                }

                // update category data
                category.Name = obj.Name.Trim();
                category.Description = obj.Description.Trim();
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = user.ID;

                // save category in database
                _dbcontext.Entry(category).State = EntityState.Modified;
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageCategory");
            }
            else
            {
                return View(obj);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCategory")]
        public ActionResult ManageCategory(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get category list
            IEnumerable<ManageCategoryViewModel> categorylist = from category in _dbcontext.NoteCategories
                                                                join user in _dbcontext.Users on category.CreatedBy equals user.ID
                                                                select new ManageCategoryViewModel
                                                                {
                                                                    ID = category.ID,
                                                                    Category = category.Name,
                                                                    Description = category.Description,
                                                                    DateAdded = category.CreatedDate.Value,
                                                                    AddedBy = user.FirstName + " " + user.LastName,
                                                                    Active = category.IsActive == true ? "YES" : "NO"
                                                                };

            // if search is not empty then get search result from categorylist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                categorylist = categorylist.Where(x => x.Category.ToLower().Contains(search) ||
                                                       x.Description.ToLower().Contains(search) ||
                                                       x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                                       x.AddedBy.ToLower().Contains(search) ||
                                                       x.Active.ToLower().Contains(search)
                                                 ).ToList();
            }

            // sort result
            categorylist = SortTableCategory(sort, categorylist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(categorylist.Count() / 5.0);

            // show result according to pagination
            categorylist = categorylist.Skip((page - 1) * 5).Take(5);

            return View(categorylist);
        }

        // sorting for category
        private IEnumerable<ManageCategoryViewModel> SortTableCategory(string sort, IEnumerable<ManageCategoryViewModel> table)
        {
            switch (sort)
            {
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
                case "Description_Asc":
                    {
                        table = table.OrderBy(x => x.Description);
                        break;
                    }
                case "Description_Desc":
                    {
                        table = table.OrderByDescending(x => x.Description);
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
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
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
        [Route("Settings/ManageCategory/Delete/{id}")]
        public ActionResult DeleteCategory(int id)
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
  
            // get category
            var category = _dbcontext.NoteCategories.Where(x => x.ID == id).FirstOrDefault();

            // check if category is null or not
            if(category == null)
            {
                return HttpNotFound();
            }

            // update category data
            category.ModifiedDate = DateTime.Now;
            category.ModifiedBy = user.ID;
            category.IsActive = false;

            // save category in database
            _dbcontext.Entry(category).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("ManageCategory");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageType/Add")]
        public ActionResult AddType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageType/Add")]
        public ActionResult AddType(AddTypeViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // create object of NoteType
                NoteType noteType = new NoteType
                {
                    Name = obj.Name.Trim(),
                    Description = obj.Description.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true

                };

                // save notetype object in database
                _dbcontext.NoteTypes.Add(noteType);
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageType");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageType/Edit/{id}")]
        public ActionResult EditType(int id)
        {
            // get type object
            var type = _dbcontext.NoteTypes.Where(x => x.ID == id).FirstOrDefault();

            // check if type is null or not
            if (type == null)
            {
                return HttpNotFound();
            }

            // create object of AddTypeViewModel
            AddTypeViewModel editType = new AddTypeViewModel
            {
                ID = type.ID,
                Name = type.Name,
                Description = type.Description
            };

            return View(editType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageType/Edit/{id}")]
        public ActionResult EditType(AddTypeViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                
                // get typpe object
                var type = _dbcontext.NoteTypes.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if type is null or not
                if (type == null)
                {
                    return HttpNotFound();
                }

                // update type data 
                type.Name = obj.Name.Trim();
                type.Description = obj.Description.Trim();
                type.ModifiedDate = DateTime.Now;
                type.ModifiedBy = user.ID;

                // save type in database
                _dbcontext.Entry(type).State = EntityState.Modified;
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageType");
            }
            else{
                return View(obj);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageType")]
        public ActionResult ManageType(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get type list
            IEnumerable<ManageTypeViewModel> typelist = from type in _dbcontext.NoteTypes
                                                        join user in _dbcontext.Users on type.CreatedBy equals user.ID
                                                        select new ManageTypeViewModel
                                                        {
                                                            ID = type.ID,
                                                            Type = type.Name,
                                                            Description = type.Description,
                                                            DateAdded = type.CreatedDate.Value,
                                                            AddedBy = user.FirstName + " " + user.LastName,
                                                            Active = type.IsActive == true ? "YES" : "NO"
                                                        };

            // if search is not empty then get search result from typelist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                typelist = typelist.Where(x => x.Type.ToLower().Contains(search) ||
                                               x.Description.ToLower().Contains(search) ||
                                               x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                               x.AddedBy.ToLower().Contains(search) ||
                                               x.Active.ToLower().Contains(search)
                                          ).ToList();
            }

            // sort results
            typelist = SortTableType(sort, typelist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(typelist.Count() / 5.0);

            // show result according to pagination
            typelist = typelist.Skip((page - 1) * 5).Take(5);

            return View(typelist);
        }

        // sorting for type
        private IEnumerable<ManageTypeViewModel> SortTableType(string sort, IEnumerable<ManageTypeViewModel> table)
        {
            switch (sort)
            {
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.Type);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.Type);
                        break;
                    }
                case "Description_Asc":
                    {
                        table = table.OrderBy(x => x.Description);
                        break;
                    }
                case "Description_Desc":
                    {
                        table = table.OrderByDescending(x => x.Description);
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
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
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
        [Route("Settings/ManageType/Delete/{id}")]
        public ActionResult DeleteType(int id)
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            
            // get type
            var type = _dbcontext.NoteTypes.Where(x => x.ID == id).FirstOrDefault();

            // check if type is null or not
            if (type == null)
            {
                return HttpNotFound();
            }

            // update type data
            type.ModifiedDate = DateTime.Now;
            type.ModifiedBy = user.ID;
            type.IsActive = false;

            // save type in database
            _dbcontext.Entry(type).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("ManageType");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCountry/Add")]
        public ActionResult AddCountry()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCountry/Add")]
        public ActionResult AddCountry(AddCountryViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // create country object
                Country country = new Country
                {
                    Name = obj.CountryName.Trim(),
                    CountryCode = obj.CountryCode.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true
                };

                // save country in database
                _dbcontext.Countries.Add(country);
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageCountry");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCountry/Edit/{id}")]
        public ActionResult EditCountry(int id)
        {
            // get country by id
            var country = _dbcontext.Countries.Where(x => x.ID == id).FirstOrDefault();

            // check if country is null or not
            if (country == null)
            {
                return HttpNotFound();
            }

            // create object of AddCountryViewModel
            AddCountryViewModel editCountry = new AddCountryViewModel
            {
                ID = country.ID,
                CountryName = country.Name,
                CountryCode = country.CountryCode
            };

            return View(editCountry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCountry/Edit/{id}")]
        public ActionResult EditCountry(AddCountryViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                
                // get country
                var country = _dbcontext.Countries.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if country is null or not
                if (country == null)
                {
                    return HttpNotFound();
                }

                // update country record
                country.Name = obj.CountryName.Trim();
                country.CountryCode = obj.CountryCode.Trim();
                country.ModifiedDate = DateTime.Now;
                country.ModifiedBy = user.ID;

                // save country in database
                _dbcontext.Entry(country).State = EntityState.Modified;
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageCountry");
            }
            else
            {
                return View(obj);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Settings/ManageCountry")]
        public ActionResult ManageCountry(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get country list
            IEnumerable<ManageCountryViewModel> countrylist = from country in _dbcontext.Countries
                                                              join user in _dbcontext.Users on country.CreatedBy equals user.ID
                                                              select new ManageCountryViewModel
                                                              {
                                                                  ID = country.ID,
                                                                  CountryName = country.Name,
                                                                  CountryCode = country.CountryCode,
                                                                  DateAdded = country.CreatedDate.Value,
                                                                  AddedBy = user.FirstName + " " + user.LastName,
                                                                  Active = country.IsActive == true ? "YES" : "NO"
                                                              };

            // if search is not empty then get search result from countrylist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                countrylist = countrylist.Where(x => x.CountryName.ToLower().Contains(search) ||
                                                     x.CountryCode.ToLower().Contains(search) ||
                                                     x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                                     x.AddedBy.ToLower().Contains(search) ||
                                                     x.Active.ToLower().Contains(search)
                                                ).ToList();
            }

            // sort results
            countrylist = SortTableCountry(sort, countrylist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(countrylist.Count() / 5.0);

            // show results according to pagination
            countrylist = countrylist.Skip((page - 1) * 5).Take(5);

            return View(countrylist);
        }

        // sorting for country
        private IEnumerable<ManageCountryViewModel> SortTableCountry(string sort, IEnumerable<ManageCountryViewModel> table)
        {
            switch (sort)
            {
                case "CountryName_Asc":
                    {
                        table = table.OrderBy(x => x.CountryName);
                        break;
                    }
                case "CountryName_Desc":
                    {
                        table = table.OrderByDescending(x => x.CountryName);
                        break;
                    }
                case "CountryCode_Asc":
                    {
                        table = table.OrderBy(x => x.CountryCode);
                        break;
                    }
                case "CountryCode_Desc":
                    {
                        table = table.OrderByDescending(x => x.CountryCode);
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
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
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
        [Route("Settings/ManageCountry/Delete/{id}")]
        public ActionResult DeleteCountry(int id)
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            
            // get country
            var country = _dbcontext.Countries.Where(x => x.ID == id).FirstOrDefault();

            // check if country is null or not
            if (country == null)
            {
                return HttpNotFound();
            }

            // update country data
            country.ModifiedDate = DateTime.Now;
            country.ModifiedBy = user.ID;
            country.IsActive = false;

            // save country in database
            _dbcontext.Entry(country).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("ManageCountry");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator/Add/{id}")]

        public ActionResult AddAdministrator()
        {
            // create object of AddAdministratorViewModel
            AddAdministratorViewModel addAdministratorViewModel = new AddAdministratorViewModel
            {
                CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList(),
                PhoneNumberCountryCode = "+91"
            };

            return View(addAdministratorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator/Add/{id}")]
        public ActionResult AddAdministrator(AddAdministratorViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists or not
                bool emailalreadyexists = _dbcontext.Users.Where(x => x.Email == obj.Email).Any();
                if (emailalreadyexists)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    obj.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                    return View(obj);
                }

                // get logged in superadmin
                var superadmin = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // create object of user
                // default password for admin is Admin@123
                // admin can change password after login
                User user = new User
                {
                    FirstName = obj.FirstName.Trim(),
                    LastName = obj.LastName.Trim(),
                    RoleID = 1,
                    Email = obj.Email.Trim(),
                    Password = "Admin@123",
                    IsEmailVerified = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = superadmin.ID,
                    IsActive = true
                };

                // save user in database
                _dbcontext.Users.Add(user);
                _dbcontext.SaveChanges();

                // get saved admin id
                var addedadmin = _dbcontext.Users.Find(user.ID);

                // crate userprofile object
                UserProfile userProfile = new UserProfile
                {
                    UserID = addedadmin.ID,
                    PhoneNumberCountryCode = obj.PhoneNumberCountryCode.Trim(),
                    PhoneNumber = obj.PhoneNumber.Trim(),
                    AddressLine1 = "NA",
                    AddressLine2 = "NA",
                    City = "NA",
                    State = "NA",
                    ZipCode = "NA",
                    Country = "NA",
                    CreatedDate = DateTime.Now,
                    CreatedBy = superadmin.ID
                };

                // save userprofile object in database
                _dbcontext.UserProfiles.Add(userProfile);
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                obj.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator/Edit/{id}")]
        public ActionResult EditAdministrator(int id)
        {
            // get logged in user and userprofile
            var user = _dbcontext.Users.Where(x => x.ID == id).FirstOrDefault();
            var userprofile = _dbcontext.UserProfiles.Where(x => x.UserID == id).FirstOrDefault();

            // check if user or userprofile is null or not
            if(user == null || userprofile == null)
            {
                return HttpNotFound();
            }

            // create object of AddAdministratorViewModel
            AddAdministratorViewModel addAdministratorViewModel = new AddAdministratorViewModel
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumberCountryCode = userprofile.PhoneNumberCountryCode,
                PhoneNumber = userprofile.PhoneNumber,
                CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList()
            };

            return View(addAdministratorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator/Edit/{id}")]
        public ActionResult EditAdministrator(AddAdministratorViewModel obj)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists or not
                bool emailalreadyexists = _dbcontext.Users.Where(x => x.Email == obj.Email && x.ID != obj.ID).Any();
                if (emailalreadyexists)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    obj.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                    return View(obj);
                }

                // get user, userprofile
                var user = _dbcontext.Users.Where(x => x.ID == obj.ID).FirstOrDefault();
                var userprofile = _dbcontext.UserProfiles.Where(x => x.UserID == obj.ID).FirstOrDefault();
                
                // get logged in superadmin
                var superadmin = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // check if user or userprofile is null or not
                if (user == null || userprofile == null)
                {
                    return HttpNotFound();
                }

                // update user data
                user.FirstName = obj.FirstName.Trim();
                user.LastName = obj.LastName.Trim();
                user.Email = obj.Email.Trim();
                user.ModifiedDate = DateTime.Now;
                user.ModifiedBy = superadmin.ID;

                // save user in database
                _dbcontext.Entry(user).State = EntityState.Modified;
                _dbcontext.SaveChanges();

                // update userprofile data
                userprofile.PhoneNumberCountryCode = obj.PhoneNumberCountryCode;
                userprofile.PhoneNumber = obj.PhoneNumber;
                userprofile.ModifiedDate = DateTime.Now;
                userprofile.ModifiedBy = superadmin.ID;

                // save userprofile in database
                _dbcontext.Entry(userprofile).State = EntityState.Modified;
                _dbcontext.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                obj.CountryCodeList = _dbcontext.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator")]
        public ActionResult ManageAdministrator(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get role id for admin
            int adminid = _dbcontext.UsersRoles.Where(x => x.Name.ToLower() == "admin").Select(x => x.ID).FirstOrDefault();

            // get list of admin
            IEnumerable<ManageAdministratorViewModel> admins = from user in _dbcontext.Users
                                                               join profile in _dbcontext.UserProfiles on user.ID equals profile.UserID
                                                               where user.RoleID == adminid
                                                               select new ManageAdministratorViewModel
                                                               {
                                                                     ID = user.ID,
                                                                     FirstName = user.FirstName,
                                                                     LastName = user.LastName,
                                                                     Email = user.Email,
                                                                     PhoneNumber = profile.PhoneNumber,
                                                                     DateAdded = user.CreatedDate.Value,
                                                                     Active = user.IsActive == true ? "YES" : "NO"
                                                               };

            // if search is not empty then get search result from admins
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                admins = admins.Where(x => x.FirstName.ToLower().Contains(search) ||
                                           x.LastName.ToLower().Contains(search) ||
                                           x.Email.ToLower().Contains(search) ||
                                           x.PhoneNumber.Contains(search) ||
                                           x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                           x.Active.ToLower().Contains(search)
                                     ).ToList();
            }

            // sort results
            admins = SortTableAdministrator(sort, admins);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(admins.Count() / 5.0);

            // show result according to pagination
            admins = admins.Skip((page - 1) * 5).Take(5);

            return View(admins);
        }

        // sorting for administrator
        private IEnumerable<ManageAdministratorViewModel> SortTableAdministrator(string sort, IEnumerable<ManageAdministratorViewModel> table)
        {
            switch (sort)
            {
                case "FirstName_Asc":
                    {
                        table = table.OrderBy(x => x.FirstName);
                        break;
                    }
                case "FirstName_Desc":
                    {
                        table = table.OrderByDescending(x => x.FirstName);
                        break;
                    }
                case "LastName_Asc":
                    {
                        table = table.OrderBy(x => x.LastName);
                        break;
                    }
                case "LastName_Desc":
                    {
                        table = table.OrderByDescending(x => x.LastName);
                        break;
                    }
                case "Email_Asc":
                    {
                        table = table.OrderBy(x => x.Email);
                        break;
                    }
                case "Email_Desc":
                    {
                        table = table.OrderByDescending(x => x.Email);
                        break;
                    }
                case "Phone_Asc":
                    {
                        table = table.OrderBy(x => x.PhoneNumber);
                        break;
                    }
                case "Phone_Desc":
                    {
                        table = table.OrderByDescending(x => x.PhoneNumber);
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
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table.ToList();
        }

        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageAdministrator/Delete/{id}")]
        public ActionResult DeleteAdministrator(int id)
        {
            // get logged in superadmin
            var superadmin = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

            // get admin user
            var admin = _dbcontext.Users.Where(x => x.ID == id).FirstOrDefault();

            // check if admin is null or not
            if (admin == null)
            {
                return HttpNotFound();
            }

            // update admin data
            admin.ModifiedDate = DateTime.Now;
            admin.ModifiedBy = superadmin.ID;
            admin.IsActive = false;

            // save admin object in database
            _dbcontext.Entry(admin).State = EntityState.Modified;
            _dbcontext.SaveChanges();

            return RedirectToAction("ManageAdministrator");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageSystemConfiguration")]
        public ActionResult ManageSystemConfiguration()
        {
            // create object of SystemConfigurationViewModel
            SystemConfigurationViewModel systemConfiguration = new SystemConfigurationViewModel();

            // get supportemail
            var supportemail = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "supportemail").FirstOrDefault();
            if(supportemail != null)
            {
                systemConfiguration.SupportEmail = supportemail.Value;
            }

            // get supportcontact
            var supportcontact = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "supportcontact").FirstOrDefault();
            if (supportcontact != null)
            {
                systemConfiguration.SupportContact = supportcontact.Value;
            }

            // get notifyemail
            var notifyemail = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "notifyemail").FirstOrDefault();
            if (notifyemail != null)
            {
                systemConfiguration.NotifyEmail = notifyemail.Value;
            }

            // get facebookurl
            var facebookurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "facebookurl").FirstOrDefault();
            if (facebookurl != null)
            {
                systemConfiguration.FacebookURL = facebookurl.Value;
            }

            // get twitterurl
            var twitterurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "twitterurl").FirstOrDefault();
            if (twitterurl != null)
            {
                systemConfiguration.TwitterURL = twitterurl.Value;
            }

            // get linkedinurl
            var linkedinurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "linkedinurl").FirstOrDefault();
            if (linkedinurl != null)
            {
                systemConfiguration.LinkedInURL = linkedinurl.Value;
            }

            // get defaultprofile
            var defaultprofile = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "defaultprofile").FirstOrDefault();
            if (defaultprofile != null)
            {
                systemConfiguration.DefaultProfileURL = defaultprofile.Value;
            }

            // get defaultnote
            var defaultnote = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "defaultnote").FirstOrDefault();
            if (defaultnote != null)
            {
                systemConfiguration.DefaultNoteURL = defaultnote.Value;
            }

            return View(systemConfiguration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        [Route("Settings/ManageSystemConfiguration")]
        public ActionResult ManageSystemConfiguration(SystemConfigurationViewModel obj)
        {
            // check if default note picture is available or not
            if (obj.DefaultNote == null && obj.DefaultNoteURL == null)
            {
                ModelState.AddModelError("DefaultNote", "Default note picture is required");
                return View(obj);
            }

            // check if default profile picture is available or not
            if (obj.DefaultProfile == null && obj.DefaultProfileURL == null)
            {
                ModelState.AddModelError("DefaultProfile", "Default profile picture is required");
                return View(obj);
            }

            if (ModelState.IsValid)
            {
                // get logged in superadmin
                var superadmin = _dbcontext.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                // create object of SystemConfiguration
                SystemConfiguration systemConfiguration = new SystemConfiguration();

                // get supportemail
                var supportemail = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "supportemail").FirstOrDefault();
                // if supportemail is null then create
                if (supportemail == null)
                {
                    systemConfiguration.Name = "supportemail";
                    systemConfiguration.Value = obj.SupportEmail.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    _dbcontext.SystemConfigurations.Add(systemConfiguration);
                    _dbcontext.SaveChanges();
                }
                // edit supportemail
                else
                {
                    if (!supportemail.Value.Equals(obj.SupportEmail))
                    {
                        supportemail.Value = obj.SupportEmail.Trim();
                        supportemail.ModifiedDate = DateTime.Now;
                        supportemail.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(supportemail).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get supportcontact
                var supportcontact = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "supportcontact").FirstOrDefault();
                // if supportcontact is null then create
                if (supportcontact == null)
                {
                    systemConfiguration.Name = "supportcontact";
                    systemConfiguration.Value = obj.SupportContact.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    _dbcontext.SystemConfigurations.Add(systemConfiguration);
                    _dbcontext.SaveChanges();
                }
                // edit supportcontact
                else
                {
                    if (!supportcontact.Value.Equals(obj.SupportContact))
                    {
                        supportcontact.Value = obj.SupportContact.Trim();
                        supportcontact.ModifiedDate = DateTime.Now;
                        supportcontact.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(supportcontact).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get notifyemail
                var notifyemail = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "notifyemail").FirstOrDefault();
                // if notifyemail is null then create
                if (notifyemail == null)
                {
                    systemConfiguration.Name = "notifyemail";
                    systemConfiguration.Value = obj.NotifyEmail.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    _dbcontext.SystemConfigurations.Add(systemConfiguration);
                    _dbcontext.SaveChanges();
                }
                // edit notifyemail
                else
                {
                    if (!notifyemail.Value.Equals(obj.NotifyEmail))
                    {
                        notifyemail.Value = obj.NotifyEmail.Trim();
                        notifyemail.ModifiedDate = DateTime.Now;
                        notifyemail.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(notifyemail).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get facebookurl
                var facebookurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "facebookurl").FirstOrDefault();
                // if facebookurl is null then create
                if (facebookurl == null)
                {
                    if (obj.FacebookURL != null)
                    {
                        systemConfiguration.Name = "facebookurl";
                        systemConfiguration.Value = obj.FacebookURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        _dbcontext.SystemConfigurations.Add(systemConfiguration);
                        _dbcontext.SaveChanges();
                    }
                }
                // edit facebookurl
                else
                {
                    if (!facebookurl.Value.Equals(obj.FacebookURL))
                    {
                        facebookurl.Value = obj.FacebookURL.Trim();
                        facebookurl.ModifiedDate = DateTime.Now;
                        facebookurl.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(facebookurl).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get twitterurl
                var twitterurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "twitterurl").FirstOrDefault();
                // if twitterurl is null then create
                if (twitterurl == null)
                {
                    if (obj.TwitterURL != null)
                    {
                        systemConfiguration.Name = "twitterurl";
                        systemConfiguration.Value = obj.TwitterURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        _dbcontext.SystemConfigurations.Add(systemConfiguration);
                        _dbcontext.SaveChanges();
                    }
                }
                // edit twitterurl
                else
                {
                    if (!twitterurl.Value.Equals(obj.TwitterURL))
                    {
                        twitterurl.Value = obj.TwitterURL.Trim();
                        twitterurl.ModifiedDate = DateTime.Now;
                        twitterurl.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(twitterurl).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get linkedinurl
                var linkedinurl = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "linkedinurl").FirstOrDefault();
                // if linkedinurl is null then create
                if (linkedinurl == null)
                {
                    if (obj.LinkedInURL != null)
                    {
                        systemConfiguration.Name = "linkedinurl";
                        systemConfiguration.Value = obj.LinkedInURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        _dbcontext.SystemConfigurations.Add(systemConfiguration);
                        _dbcontext.SaveChanges();
                    }
                }
                // edit linkedinurl
                else
                {
                    if (!linkedinurl.Value.Equals(obj.LinkedInURL))
                    {
                        linkedinurl.Value = obj.LinkedInURL.Trim();
                        linkedinurl.ModifiedDate = DateTime.Now;
                        linkedinurl.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(linkedinurl).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get defaultprofile
                var defaultprofile = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "defaultprofile").FirstOrDefault();
                // if defaultprofile is null then create
                if (defaultprofile == null)
                {
                    systemConfiguration.Name = "defaultprofile";

                    if (obj.DefaultProfile != null)
                    {
                        string fileextension = System.IO.Path.GetExtension(obj.DefaultProfile.FileName);
                        string newfilename = "profile" + fileextension;
                        string profilepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.DefaultProfile.SaveAs(path);

                        systemConfiguration.Value = profilepicturepath + newfilename;
                    }

                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    _dbcontext.SystemConfigurations.Add(systemConfiguration);
                    _dbcontext.SaveChanges();
                }
                // edit defaultprofile
                else
                {
                    // check if user uploaded defaultprofile
                    if (obj.DefaultProfile != null)
                    {
                        // check if there is already profile picture is available or not
                        // if available then we have to delete
                        if (defaultprofile.Value != null)
                        {
                            string previouspath = Server.MapPath(defaultprofile.Value);
                            FileInfo file = new FileInfo(previouspath);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }

                        string fileextension = System.IO.Path.GetExtension(obj.DefaultProfile.FileName);
                        string newfilename = "profile" + fileextension;
                        string profilepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.DefaultProfile.SaveAs(path);

                        defaultprofile.Value = profilepicturepath + newfilename;

                        defaultprofile.ModifiedDate = DateTime.Now;
                        defaultprofile.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(defaultprofile).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                // get defaultnote
                var defaultnote = _dbcontext.SystemConfigurations.Where(x => x.Name.ToLower() == "defaultnote").FirstOrDefault();
                // if defaultnote is null then create
                if (defaultnote == null)
                {
                    systemConfiguration.Name = "defaultnote";

                    if (obj.DefaultNote != null)
                    {
                        string fileextension = System.IO.Path.GetExtension(obj.DefaultNote.FileName);
                        string newfilename = "note" + fileextension;
                        string notepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(notepicturepath);
                        string path = Path.Combine(Server.MapPath(notepicturepath), newfilename);
                        obj.DefaultNote.SaveAs(path);

                        systemConfiguration.Value = notepicturepath + newfilename;
                    }

                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    _dbcontext.SystemConfigurations.Add(systemConfiguration);
                    _dbcontext.SaveChanges();
                }
                // edit defaultnote
                else
                {
                    // check if user uploaded defaultnote
                    if (obj.DefaultNote != null)
                    {
                        // check if there is already note picture is available or not
                        // if available then we have to delete
                        if (defaultnote.Value != null)
                        {
                            string previouspath = Server.MapPath(defaultnote.Value);
                            FileInfo file = new FileInfo(previouspath);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }

                        string fileextension = System.IO.Path.GetExtension(obj.DefaultNote.FileName);
                        string newfilename = "note" + fileextension;
                        string notepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(notepicturepath);
                        string path = Path.Combine(Server.MapPath(notepicturepath), newfilename);
                        obj.DefaultNote.SaveAs(path);

                        defaultnote.Value = notepicturepath + newfilename;

                        defaultnote.ModifiedDate = DateTime.Now;
                        defaultnote.ModifiedBy = superadmin.ID;

                        _dbcontext.Entry(defaultprofile).State = EntityState.Modified;
                        _dbcontext.SaveChanges();
                    }
                }

                return RedirectToAction("ManageSystemConfiguration");
            }
            else
            {
                return View(obj);
            }
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