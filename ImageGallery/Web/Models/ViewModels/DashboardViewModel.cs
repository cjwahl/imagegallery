using System.Collections.Generic;

namespace ImageGallery.Models
{
    public class DashboardViewModel
    {
        public string ErrorMessage { get; set; }
        public List<Album> Albums { get; set; }
        public List<int> Years { get; set; }
             
    }
}