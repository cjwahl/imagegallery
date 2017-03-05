using FamilyReunionGallery.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var model = new DashboardViewModel();
                model.Albums = HelperFunctions.GetAlbumList();
                model.Years = new List<int>();
                foreach (var album in model.Albums)
                {
                    model.Years.Add(Convert.ToInt32(album.Year));
                }

                model.Years = model.Years.Distinct().OrderByDescending(i => i).ToList();

                return View(model);
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

                model.Albums = HelperFunctions.GetAlbumList();
                model.Years = new List<int>();
                foreach (var album in model.Albums)
                {
                    model.Years.Add(Convert.ToInt32(album.Year));
                }
                model.Years = model.Years.Distinct().OrderByDescending(i => i).ToList();
                return View(model);
            }
            else
            {
                model.ErrorMessage = "Invaild Passcode";
                return View("~/Views/Home/Index.cshtml", model);
            }
        }

        public ActionResult Gallery(string album)
        {
            var model = new GalleryViewModel(album);
            model.Album = HelperFunctions.GetAlbumImages(model.Album);

            if ((string)Session["ValidSession"] == "valid")
            {
                return View(model);
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }
        }

        public ActionResult Upload()
        {
            var model = new UploadViewModel();
            model.AlbumDropDown = new List<SelectListItem>();
            model.Albums = HelperFunctions.GetAlbumList();
            foreach (var album in model.Albums)
            {
                model.AlbumDropDown.Add(new SelectListItem() { Text = album.AlbumTitle, Value = album.Directory });
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

        [HttpPost]
        public ActionResult UploadFile(string Album)
        {
            var resolveRequest = HttpContext.Request;
            resolveRequest.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonString = new StreamReader(resolveRequest.InputStream).ReadToEnd();
            var split = jsonString.Split('\n');
            var directoryName = split[3].Replace("\r", "");
            bool isSavedSuccessfully = true;
            try
            {
                foreach (string image in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[image];
                    var fileName = Path.GetFileName(file.FileName);
                    var fullDirectory = new DirectoryInfo(string.Format("{0}Content/images/temp/fulls", Server.MapPath("/")));

                    HelperFunctions.UploadImage(file, fullDirectory, directoryName, fileName);

                    var thumbDirectory = new DirectoryInfo(string.Format("{0}Content/images/temp/thumbs", Server.MapPath("/")));

                    HelperFunctions.UploadImage(file, thumbDirectory, directoryName, fileName);
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(9, "Error", ex.ToString());
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = "Success" });
            }
            else
            {
                return Json(new { Message = "Error saving file" });
            }
        }

        public ActionResult NewAlbum()
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
        public ActionResult NewAlbum(HttpPostedFileBase file, FormCollection form)
        {
            var model = new NewAlbumViewModel();
            bool success = false;
            var album = new Album();
            if (form != null)
            {
                album.AlbumTitle = form["title"].ToString();
                album.Year = form["year"].ToString();
                album.Directory = HelperFunctions.MakeValidDirectoryName(album.AlbumTitle);
                if (!HelperFunctions.CheckIfAlbumExists(album.AlbumTitle))
                {
                    success = HelperFunctions.InsertNewAlbum(album);
                }
                else
                {
                    model.ErrorMessage = "Album Name already exists. Try adding the year after it.";
                }
            }
            if (success)
            {
                HelperFunctions.CreateS3Folder(album.Directory);

                if (file != null)
                {
                    HttpPostedFileBase image = file;

                    var name = Path.GetFileName(image.FileName);
                    var directory = new DirectoryInfo(string.Format("{0}Content/images/temp/", Server.MapPath("/")));
                    
                   HelperFunctions.UploadImage(image, directory, "Dashboard", string.Concat(album.Directory, ".jpg"));
                    
                }
            }

            if ((string)Session["ValidSession"] == "valid" && success)
            {
                return View();
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }
        }
    }
}