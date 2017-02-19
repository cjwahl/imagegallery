using FamilyReunionGallery.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyReunionGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["ValidSession"] = "notvalid";
            var model = new DashboardViewModel();
            return View(model);
        }
        public ActionResult Dashboard()
        {
            if ((string)Session["ValidSession"] == "valid")
            {
                return View();
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }
            
        }
        [HttpPost]
        public ActionResult Dashboard(FormCollection form)
        {
            var model = new DashboardViewModel();
            if (form["passcode"] == "crazypassword")
            {
                Session["ValidSession"] = "valid";
                return View();
            }
            else
            {
                model.ErrorMessage = "Invaild Passcode";
                return View("~/Views/Home/Index.cshtml", model);
            }
            
        }
        public ActionResult JoelleBday()
        {
            var model = new GalleryViewModel();
            model.FullImages = new List<FileInfo>();
            model.ThumbImages = new List<FileInfo>();
            model.AlbumnImagePath = "JoelleBday";
            model.AlbumTitle = "Joelle's Birthday Camping Trip";
            foreach (var imgPath in Directory.GetFiles(Server.MapPath("~/Content/images/JoelleBday/fulls"), "*.JPG"))
            {
                var img = new FileInfo(imgPath);
                model.FullImages.Add(img);
            }
            foreach (var imgPath in Directory.GetFiles(Server.MapPath("~/Content/images/JoelleBday/thumbs"), "*.JPG"))
            {
                var img = new FileInfo(imgPath);
                model.ThumbImages.Add(img);
            }
            if ((string)Session["ValidSession"] == "valid")
            {
                return View(model);
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }

        }
    }
}