using System.Collections.Generic;
using System.Web.Mvc;

namespace FamilyReunionGallery.Models
{
    public class EditAlbumViewModel
    {
        public EditAlbumViewModel()
        {
            Albums = HelperFunctions.GetAlbumList();
            AlbumDropDown = new List<SelectListItem>();
            AlbumDropDown.Add(new SelectListItem() { Text = "Select an Album", Value = "" });
            foreach(var album in Albums)
            {
                var listItem = new SelectListItem();
                listItem.Text = album.AlbumTitle;
                listItem.Value = album.Directory;
                AlbumDropDown.Add(listItem);
            }

            Album = new Album();
        }
        public List<SelectListItem> AlbumDropDown { get; set; }
        public string SelectedAlbum { get; set; }
        public List<Album> Albums { get; set; }
        public Album Album { get; set; }
    }
}