using ImageGallery.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageGallery.Controllers
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
                    model.Years.Add(album.Year);
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
                    model.Years.Add(album.Year);
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
        public JsonResult UploadFile(string Album)
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

                    var fileName = string.Format("{0}.jpg", Guid.NewGuid());

                    HelperFunctions.UploadImage(file, "full", directoryName, fileName);

                    HelperFunctions.UploadImage(file, "thumb", directoryName, fileName);
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
                album.Year = !string.IsNullOrEmpty(form["year"].ToString()) ? Convert.ToInt32(form["year"].ToString()) : Convert.ToInt32("2017");
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
                    
                   HelperFunctions.UploadImage(image, "", "Dashboard", string.Concat(album.Directory, ".jpg"));
                    
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

        public ActionResult EditAlbum()
        {
            if ((string)Session["ValidSession"] == "valid")
            {
                var model = new EditAlbumViewModel();

                return View(model);
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }
        }

        public ActionResult EditAlbumResult(EditAlbumViewModel model)
        {

            if ((string)Session["ValidSession"] == "valid")
            {
                foreach(var album in model.Albums)
                {
                    if(album.Directory == model.SelectedAlbum)
                    {
                        model.Album = album;
                    }
                }

                return PartialView(model);
            }
            else
            {
                return View("~/Views/Home/Index.cshtml", new DashboardViewModel());
            }
        }

        [HttpPost]
        public JsonResult Delete(string album)
        {
            var isDeleted = true;
            try
            {
                HelperFunctions.DeleteImage(album);
            }
            catch(Exception ex)
            {
                Debugger.Log(9, "Error", ex.ToString());
                isDeleted = false;
            }

            if (isDeleted)
            {
                return Json(new { Message = "Success" });
            }
            else
            {
                return Json(new { Message = "Error deleting file" });
            }
        }
    }
}