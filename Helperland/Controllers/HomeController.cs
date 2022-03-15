﻿using Helperland.Data;
using Helperland.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Helperland.Controllers
{
    public class HomeController : Controller
    {

        private readonly HelperlandContext _db;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly ILogger<HomeController> _logger;

        // Dependency Injection
        public HomeController(ILogger<HomeController> logger, HelperlandContext db, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _db = db;
            this.hostEnvironment = hostEnvironment;
        }

        //====================================
        //          Public-Pages
        //====================================

        //Index
        public IActionResult Index()
        {

            
            ViewBag.IsHomePage = true;
            return View();
        }

        //About
        public IActionResult About()
        {
            return View();
        }

        //Contact Us
        public IActionResult Contact()
        {
            return View();
        }

        //Contact Us (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactU obj)
        {

            string uniqueFileName = null;
            if (obj.UploadFile != null)
            {
                string folderPath = Path.Combine(hostEnvironment.WebRootPath, "Uploads/ContactUsAttachments");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + obj.UploadFile.FileName;
                string filePath = Path.Combine(folderPath, uniqueFileName);
                obj.UploadFile.CopyTo(new FileStream(filePath, FileMode.Create));
            }

            obj.UploadFileName = uniqueFileName;
            obj.Name = HttpContext.Request.Form["FName"] + " " + HttpContext.Request.Form["LName"];
            obj.CreatedOn = DateTime.Now;
            _db.ContactUs.Add(obj);
            _db.SaveChanges();
            TempData["Msg"] = "Response has been recorded";
            return RedirectToAction("Contact");
        }

        //FAQs
        public IActionResult Faq()
        {
            return View();
        }

        //Prices
        public IActionResult Prices()
        {
            return View();
        }

        //Login
        [HttpPost]
        public IActionResult Login(User obj)
        {
            User user = _db.Users.Where(x => x.Email == obj.Email && x.Password == obj.Password).SingleOrDefault();
            if (user != null)
            {
                var username = user.FirstName + " " + user.LastName;
                HttpContext.Session.SetInt32("UserID_Session", user.UserId);
                HttpContext.Session.SetString("UserName_Session", username);
                int usertype = user.UserTypeId;

                switch (usertype)
                {
                    case 1:
                        return RedirectToAction("Index");
                    case 2:
                        return RedirectToAction("Index");
                    case 3:
                        return RedirectToAction("CustomerDashboard");
                    default:
                        return RedirectToAction("Index");
                }

            }
            else
            {
                TempData["Msg4Popup"] = "Username or Password is Incorrect. Please try again";
                return RedirectToAction("Index");
            }
        }

        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserID_Session");
            HttpContext.Session.Remove("UserName_Session");
            return RedirectToAction("Index");
        }

        //ForgotPassword
        [HttpPost]
        public IActionResult ForgotPassword(User e)
        {
            var _objuserdetail = (from i in _db.Users where i.Email == e.Email select i).SingleOrDefault();

            if (_objuserdetail != null)
            {
                string BaseUrl = string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host);
                var ResetUrl = $"{BaseUrl}/Home/ResetPassword/" + _objuserdetail.UserId;
                MailMessage msg = new MailMessage();
                msg.To.Add(_objuserdetail.Email);
                msg.From = new MailAddress("getpaswordback@gmail.com");
                msg.Subject = "Reset Password - Helperland";
                msg.Body = "Hello " + _objuserdetail.FirstName + ",\n\nYour can reset your password by clicking the link below \n" + ResetUrl + "\nThank you for visiting Helperland \n\nRegards,\nHelperland Team";

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.Port = 587;


                NetworkCredential NC = new NetworkCredential("getpaswordback@gmail.com", "Demo@123");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NC;
                smtp.Send(msg);
                TempData["Msg4Popup"] = "Mail sent successfully";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Msg4Popup"] = "Account not found based on this Email. Please try again with valid Email Id";
                return RedirectToAction("Index");
            }
        }

        //Reset Password
        [HttpGet]
        public IActionResult ResetPassword(int? id)
        
        {
            ViewData["ResetUser"] = id;
            User user = _db.Users.Where(x => x.UserId == id).FirstOrDefault();
            return View(user);
        }

        //Reset Password (POST)
        [HttpPost]
        public IActionResult ResetPassword(User obj)
        {
            if (ModelState.IsValid)
            {
                User user = _db.Users.Where(x => x == obj).FirstOrDefault();
                user.Password = obj.Password;
                user.ModifiedBy = obj.UserId;
                user.ModifiedDate = DateTime.Now; 
                _db.Users.Update(user);
                _db.SaveChanges();
                TempData["Msg4Popup"] = "Password has been changed successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        //====================================
        //          Customer-Pages
        //====================================

        //Customer SignUp
        public IActionResult CustomerSignup()
        {
            return View();
        }

        //Customer SignUp (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerSignup(User obj)
        {
            if (ModelState.IsValid)
            {
                if (!_db.Users.Any(x => x.Email == obj.Email))
                {
                    obj.UserTypeId = 3;
                    obj.IsRegisteredUser = false;
                    obj.WorksWithPets = false;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedDate = DateTime.Now;
                    obj.ModifiedBy = 0;
                    obj.IsApproved = false;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    _db.Users.Add(obj);
                    _db.SaveChanges();
                    TempData["SuccessMsg"] = "Account created";
                    return RedirectToAction("CustomerSignup");
                }
                else
                {
                    TempData["ErrorMsg"] = "Email already exists. Try using different email id.";
                }
            }
            return View();
        }

        //Customer Dashboard
        public IActionResult CustomerDashboard()
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            List<ServiceRequest> obj = new List<ServiceRequest>();
            obj = _db.ServiceRequests.Where(x => x.UserId == userid && x.Status == 0).ToList();
            return View(obj);
        }

        //Customer Dashboard Modal View
        public IActionResult SRDashModal(int id = 5)
        {
            ServiceRequestExtra[] obj = _db.ServiceRequestExtras.Where(x => x.ServiceRequestId == id).ToArray();
            List<int> extras = new List<int>();
            foreach (var item in obj)
            {
               extras.Add(item.ServiceExtraId);
            }
            var query = (from SR in _db.ServiceRequests
                        join SRaddress in _db.ServiceRequestAddresses on SR.ServiceRequestId equals SRaddress.ServiceRequestId
                        //join SRextra in _db.ServiceRequestExtras on SRaddress.ServiceRequestId equals SRextra.ServiceRequestId
                        where SR.ServiceRequestId == id
                        select new CustomModel
                        {
                            ServiceRequestId = SR.ServiceRequestId,
                            ServiceId = SR.ServiceId,
                            ServiceStartDate = SR.ServiceStartDate,
                            ServiceHours = SR.ServiceHours,
                            Comments = SR.Comments,
                            HasPets = SR.HasPets,
                            TotalCost = SR.TotalCost,

                            AddressLine1 = SRaddress.AddressLine1,
                            AddressLine2 = SRaddress.AddressLine2,
                            City = SRaddress.City,
                            State = SRaddress.State,
                            PostalCode = SRaddress.PostalCode,
                            Mobile = SRaddress.Mobile,

                            ServiceExtraId = extras
                        }).Single();

            return View(query);
        }

        //Customer Dashboard Cancel Request
        public bool CustomerDashboardCancelRequest([FromBody] ServiceRequest sr)
        {
            ServiceRequest obj = _db.ServiceRequests.Where(x => x.ServiceRequestId == sr.ServiceRequestId).FirstOrDefault();
            obj.Status = 3;
            obj.Comments = sr.Comments;
            _db.ServiceRequests.Update(obj);
            _db.SaveChanges();
            return true;
        }

        //Customer Settings Tab
        public IActionResult CustomerSettings()
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            User user = _db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            return View(user);
        }

        //Customer Settings Tab (POST)
        [HttpPost]
        public IActionResult CustomerSettings(User user)
        {
            User obj = _db.Users.Where(x => x.UserId == user.UserId).FirstOrDefault();
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            if (ModelState.IsValid)
            {
                obj.FirstName = user.FirstName;
                obj.LastName = user.LastName;
                obj.Mobile = user.Mobile;
                obj.DateOfBirth = user.DateOfBirth;
                _db.Users.Update(obj);
                _db.SaveChanges();

                var username = user.FirstName + " " + user.LastName;
                HttpContext.Session.SetString("UserName_Session", username);

                TempData["Msg4Popup"] = "Profile Updated Successfully";
            }

            return View(obj);
        }

        //Customer Settings Password Change (POST)
        public bool CustomerPasswordChange([FromBody] User passwords)
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            User user = _db.Users.Where(x => x.UserId == userid).FirstOrDefault();

            if (user.Password != passwords.Password)
            {
                return false;
            }
            else
            {
                try
                {
                    user.Password = passwords.ConfirmPassword;
                    _db.Users.Update(user);
                    _db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        //Customer Address in Settings Tab
        public IActionResult CustomerAddress()
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            List<UserAddress> obj = _db.UserAddresses.Where(x => x.UserId == userid).ToList();
            return View(obj);
        }

        //Customer Address Add or Edit
        public IActionResult CustomerAddressAddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View();
            }
            else
            {
                UserAddress obj = _db.UserAddresses.Where(x => x.AddressId == id).FirstOrDefault();
                return View(obj);
            }
        }

        //Customer Address Add or Edit (POST)
        [HttpPost]
        public string CustomerAddressAddOrEdit(UserAddress obj)
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            User user = _db.Users.Where(x => x.UserId == userid).FirstOrDefault();
            if (obj.AddressId == 0)
            {
                obj.UserId = userid;
                obj.Email = user.Email;
                obj.IsDefault = false;
                obj.IsDeleted = false;
                _db.UserAddresses.Add(obj);
                _db.SaveChanges();
            }
            else
            {
                _db.UserAddresses.Update(obj);
                _db.SaveChanges();
            }
            return "true";
        }

        //Customer Address Delete (POST)
        [HttpPost]
        public string CustomerAddressDelete(int id)
        {
            UserAddress obj = _db.UserAddresses.Where(x => x.AddressId == id).FirstOrDefault();
            _db.UserAddresses.Remove(obj);
            _db.SaveChanges();
            return "true";
        }

        //Customer Service History Tab
        public IActionResult CustomerServiceHistory()
        {
            int userid = (int)HttpContext.Session.GetInt32("UserID_Session");
            List<ServiceRequest> obj = _db.ServiceRequests.Where(x => x.UserId == userid && x.Status == 2 || x.Status == 3).ToList();
            return View(obj);
        }

        //Customer Service History Tab
        public IActionResult CustomerFavouritePros()
        {
            return View();
        }

        //====================================
        //           Book Service
        //====================================
        public IActionResult BookService()
        {
            var userID = HttpContext.Session.GetInt32("UserID_Session");
            var userName = HttpContext.Session.GetInt32("UserName_Session");

            if (userID != null)
            {
                return View();
            }
            TempData["Msg4Popup"] = "You have to login to Book a Service.";
            return RedirectToAction("Index");
        }

        //Book Service (POST)
        [HttpPost]
        public string BookServiceRequest([FromBody] ServiceRequest book)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserID_Session");
            UserAddress address = _db.UserAddresses.Where(x => x.AddressId == book.AddressId).SingleOrDefault();
            book.UserId = userID;
            book.ServiceId = 1000;
            book.PaymentDue = true;
            book.CreatedDate = DateTime.Now;
            book.ModifiedDate = DateTime.Now;
            book.ModifiedBy = userID;
            book.Distance = 10;
            book.Status = 0;                // 0:New  1:Pending  2:Completed  3:Cancelled
            
            _db.ServiceRequests.Add(book);
            _db.SaveChanges();

            book.ServiceId = 1000 + book.ServiceRequestId;
            _db.ServiceRequests.Update(book);
            _db.SaveChanges();

            ServiceRequestAddress requestAddress = new ServiceRequestAddress();
            requestAddress.ServiceRequestId = book.ServiceRequestId;
            requestAddress.AddressLine1 = address.AddressLine1;
            requestAddress.AddressLine2 = address.AddressLine2;
            requestAddress.City = address.City;
            requestAddress.State = address.State;
            requestAddress.PostalCode = address.PostalCode;
            requestAddress.Email = address.Email;
            requestAddress.Mobile = address.Mobile;

            _db.ServiceRequestAddresses.Add(requestAddress);
            _db.SaveChanges();

            for (int i = 0; i < book.Extras.Length; i++)
            {
                if (book.Extras[i] == true)
                {
                    ServiceRequestExtra extras = new ServiceRequestExtra();
                    extras.ServiceExtraId = i+1;
                    extras.ServiceRequestId = book.ServiceRequestId;
                    _db.ServiceRequestExtras.Add(extras);
                }
            }
            _db.SaveChanges();



            MailMessage msg = new MailMessage();
            
            msg.From = new MailAddress("getpaswordback@gmail.com");
            msg.Subject = "New Service - Helperland";
            msg.Body = "Hello Service Provider,\n\nNew Service Request is Available in your area\n \nThank you for visiting Helperland \n\nRegards,\nHelperland Team";

            var emailList = from u in _db.Users
                            where u.UserTypeId == 2
                            select u.Email;

            foreach (var i in emailList)
            {
                msg.To.Add(new MailAddress(i));
            }

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.Port = 587;


            NetworkCredential NC = new NetworkCredential("getpaswordback@gmail.com", "Demo@123");
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NC;
            smtp.Send(msg);

            return book.ServiceId.ToString();
        }

        //Validate Zip Code (Tab-1)
        public string ValidatePostalCode(string postalcode)
        {
            if (postalcode == null)
            {
                return "false";
            }
            var PostalCode = _db.Users.Where(x => x.ZipCode == postalcode).SingleOrDefault();
            string IsValidated;
            if (PostalCode != null)
            {
                IsValidated = "true";
            }
            else
            {
                IsValidated = "false";
            }
            return IsValidated;

        }

        //Fetch User Address Page (Tab-3)
        public IActionResult BookServiceAddress()
        {
            System.Threading.Thread.Sleep(2000);
            var UserID = HttpContext.Session.GetInt32("UserID_Session");
            List<UserAddress> allAddress = new List<UserAddress>();
            allAddress = _db.UserAddresses.Where(x => x.UserId == UserID).ToList();

            return View(allAddress);
        }

        //Add Address (Tab-3)
        public string AddAddress([FromBody] UserAddress address)
        {
            int UserID = (int)HttpContext.Session.GetInt32("UserID_Session");
            User user = _db.Users.Where(x => x.UserId == UserID).SingleOrDefault();
            if (address == null)
            {
                return "false";
            }
            else
            {
                address.UserId = UserID;
                address.Email = user.Email;
                address.IsDefault = false;
                address.IsDeleted = false;
                _db.UserAddresses.Add(address);
                _db.SaveChanges();
                return "true";
            }
        }

        //====================================
        //      Service Provider-Pages
        //====================================
        
        //Service Provider SignUp
        public IActionResult BecomeAPro()
        {
            return View();
        }

        //Service Provider SignUp (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BecomeAPro(User obj)
        {
            if (ModelState.IsValid)
            {
                if (!_db.Users.Any(x => x.Email == obj.Email))
                {
                    obj.UserTypeId = 2;
                    obj.IsRegisteredUser = false;
                    obj.WorksWithPets = false;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedDate = DateTime.Now;
                    obj.ModifiedBy = 0;
                    obj.IsApproved = false;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    _db.Users.Add(obj);
                    _db.SaveChanges();
                    TempData["SuccessMsg"] = "Account Created Successfully";
                    return RedirectToAction("BecomeAPro");
                }
                else
                {
                    TempData["ErrorMsg"] = "Email Already Exists. Try using different email id";
                }
            }
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}