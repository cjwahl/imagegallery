using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FamilyReunionGallery.Models
{
    public class DashboardViewModel
    {
        public string ErrorMessage { get; set; }
        public List<Album> Albums { get; set; }
        public List<int> Years { get; set; }
             
    }
}