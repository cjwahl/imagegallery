using FamilyReunionGallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyReunionGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Gallery()
        {
            var model = new GalleryViewModel();
            model.IsAuthenticated = false;

            if (model.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return View("~/Views/Home/Index.cshtml");
            }
            
        }
    }
}