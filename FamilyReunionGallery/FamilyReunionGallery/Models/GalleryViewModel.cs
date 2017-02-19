using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyReunionGallery.Models
{
    public class GalleryViewModel
    {
        public string AlbumnImagePath { get; set; }
        public string AlbumTitle { get; set; }
        public List<FileInfo> FullImages { get; set; }
        public List<FileInfo> ThumbImages { get; set; }
    }
}