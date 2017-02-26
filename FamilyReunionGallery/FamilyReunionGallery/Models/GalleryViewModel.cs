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
            Data = HelperFunctions.GetSqlData();

            foreach(DataRow item in Data.Rows)
            {
                if(item["Name"].ToString() == title)
                {
                    AlbumTitle = item["Name"].ToString();
                    AlbumnImagePath = item["Directory"].ToString();
                    Year = item["Year"].ToString();
                }
            }
        }

        public string AlbumnImagePath { get; set; }
        public string AlbumTitle { get; set; }
        public string Year { get; set; }
        public List<FileInfo> FullImages { get; set; }
        public List<FileInfo> ThumbImages { get; set; }
        public DataTable Data { get; set; }

        
    }
}