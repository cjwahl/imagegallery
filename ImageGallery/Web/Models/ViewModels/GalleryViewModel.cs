using System;
using System.Data;

namespace ImageGallery.Models
{
    public class GalleryViewModel
    {
        public GalleryViewModel(string title)
        {
            var data = HelperFunctions.GetSqlData();
            Album = new Album();
            foreach(DataRow item in data.Rows)
            {
                if(item["Name"].ToString() == title)
                {
                    Album.AlbumTitle = item["Name"].ToString();
                    Album.Directory = item["Directory"].ToString();
                    Album.Year = item["Year"].ToString() != "" ? Convert.ToInt32(item["Year"].ToString()) : 2017;
                }
            }
        }

        public Album Album { get; set; }

        
    }
}