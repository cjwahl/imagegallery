using Amazon.S3.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace FamilyReunionGallery.Models
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
                    Album.Year = item["Year"].ToString();
                }
            }
        }

        public Album Album { get; set; }

        
    }
}