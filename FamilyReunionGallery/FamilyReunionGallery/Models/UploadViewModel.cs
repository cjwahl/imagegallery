using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyReunionGallery.Models
{
    public class UploadViewModel
    {

        public List<SelectListItem> AlbumDropDown { get; set; }
        public string SelectedAlbum { get; set; }
        public List<Album> Albums { get; set; }
        
    }
}