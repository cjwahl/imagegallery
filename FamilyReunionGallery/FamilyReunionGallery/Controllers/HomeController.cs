using FamilyReunionGallery.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
                model.Years.OrderByDescending(i => i);

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
                model.Years = model.Years.OrderByDescending(i => i).ToList();
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
            model.FullImages = new List<FileInfo>();
            model.ThumbImages = new List<FileInfo>();

            var fullpath = String.Format("~/Content/images/{0}/fulls", model.AlbumnImagePath);
            var thumbpath = String.Format("~/Content/images/{0}/thumbs", model.AlbumnImagePath);
            if (Directory.GetFiles(Server.MapPath(fullpath), "*.JPG").Any() && Directory.GetFiles(Server.MapPath(thumbpath), "*.JPG").Any())
            {
                foreach (var imgPath in Directory.GetFiles(Server.MapPath(fullpath), "*.JPG"))
                {
                    var img = new FileInfo(imgPath);
                    model.FullImages.Add(img);
                }
                foreach (var imgPath in Directory.GetFiles(Server.MapPath(thumbpath), "*.JPG"))
                {
                    var img = new FileInfo(imgPath);
                    model.ThumbImages.Add(img);
                }
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

        public ActionResult Upload()
        {
            var model = new UploadViewModel();
            model.AlbumDropDown = new List<SelectListItem>();
            model.Albums = HelperFunctions.GetAlbumList();
            foreach (var directory in Directory.GetDirectories(Server.MapPath("~/Content/images")))
            {
                var dir = directory.Split('\\').Last();
                if (dir != "Dashboard")
                {
                    model.AlbumDropDown.Add(new SelectListItem() { Text = HelperFunctions.GetAlbumName(dir, model.Albums), Value = dir });
                }
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
            string fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0 && file.ContentType.Contains("image"))
                    {
                        var fullDirectory = new DirectoryInfo(string.Format("{0}Content/images/{1}/fulls", Server.MapPath("/"), directoryName));

                        var name = Path.GetFileName(file.FileName);

                        bool directoryExists = Directory.Exists(fullDirectory.ToString());
                        bool fileExists = System.IO.File.Exists(string.Concat(fullDirectory.ToString(), "\\", name));

                        var path = string.Format("{0}\\{1}", fullDirectory.ToString(), file.FileName);
                        if (!directoryExists)
                        {
                            Directory.CreateDirectory(fullDirectory.ToString());
                        }
                        if (!fileExists)
                        {
                            file.SaveAs(path);
                        }

                        Stream BitmapStream = System.IO.File.Open(path, FileMode.Open);

                        Image img = Image.FromStream(BitmapStream);
                        BitmapStream.Dispose();
                        if (img.Width > img.Height)
                        {
                            img = HelperFunctions.ResizeImage(img, 1024, 768);
                        }
                        else
                        {
                            img = HelperFunctions.ResizeImage(img, 768, 1024);
                        }

                        img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

                        var thumbDirectory = new DirectoryInfo(string.Format("{0}Content/images/{1}/thumbs", Server.MapPath("/"), directoryName));

                        name = Path.GetFileName(file.FileName);

                        directoryExists = Directory.Exists(thumbDirectory.ToString());
                        fileExists = System.IO.File.Exists(string.Concat(thumbDirectory.ToString(), "\\", name));

                        path = string.Format("{0}\\{1}", thumbDirectory.ToString(), file.FileName);
                        if (!directoryExists)
                        {
                            Directory.CreateDirectory(thumbDirectory.ToString());
                        }
                        if (!fileExists)
                        {
                            file.SaveAs(path);
                        }

                        BitmapStream = System.IO.File.Open(path, FileMode.Open);

                        img = Image.FromStream(BitmapStream);
                        BitmapStream.Dispose();

                        if (img.Width > img.Height)
                        {
                            img = HelperFunctions.ResizeImage(img, 642, 482);
                        }
                        else
                        {
                            img = HelperFunctions.ResizeImage(img, 482, 642);
                        }

                        img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(9, "Error", ex.ToString());
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName });
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
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
                album.Name = form["title"].ToString();
                album.Year = form["year"].ToString();
                album.Directory = HelperFunctions.MakeValidDirectoryName(album.Name);
                if (!HelperFunctions.CheckIfAlbumExists(album.Name))
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
                var directory = new DirectoryInfo(string.Format("{0}Content/images/{1}/", Server.MapPath("/"), album.Directory));
                var directoryExists = Directory.Exists(directory.ToString());

                if (!directoryExists)
                {
                    Directory.CreateDirectory(directory.ToString());
                    directory = new DirectoryInfo(string.Format("{0}Content/images/{1}/fulls", Server.MapPath("/"), album.Directory));
                    Directory.CreateDirectory(directory.ToString());
                    directory = new DirectoryInfo(string.Format("{0}Content/images/{1}/thumbs", Server.MapPath("/"), album.Directory));
                    Directory.CreateDirectory(directory.ToString());
                }
                if (file != null)
                {
                    HttpPostedFileBase image = file;
                    directory = new DirectoryInfo(string.Format("{0}Content/images/Dashboard/", Server.MapPath("/")));

                    var name = Path.GetFileName(image.FileName);

                    bool fileExists = System.IO.File.Exists(string.Concat(directory.ToString(), "\\", name));
                    var path = string.Format("{0}\\{1}", directory.ToString(), string.Concat(album.Directory, ".jpg"));

                    if (!fileExists)
                    {
                        image.SaveAs(path);
                    }
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