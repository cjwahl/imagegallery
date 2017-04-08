using Amazon.S3.Model;
using System.Collections.Generic;

namespace ImageGallery.Models
{
    public class Album
    {
        public Album()
        {
            FullImages = new List<S3Object>();
            ThumbImages = new List<S3Object>();
        }
        public string AlbumTitle { get; set; }
        public string Directory { get; set; }
        public int Year { get; set; }
        public S3Object DirectoryImage { get; set; }
        public List<S3Object> FullImages { get; set; }
        public List<S3Object> ThumbImages { get; set; }
    }
}