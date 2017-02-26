using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FamilyReunionGallery.Models
{
    public class HelperFunctions
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static string GetAlbumName(string directoryName, List<Album> albumList)
        {
            foreach (var album in albumList)
            {
                if (album.Directory == directoryName)
                {
                    return album.Name;
                }
            }

            return "";
        }

        public static DataTable GetSqlData()
        {
            var dt = new DataTable();
            var cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(cs))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM Album", connection);
                SqlDataAdapter da = new SqlDataAdapter(command);

                da.Fill(dt);
            }

            return dt;
        }

        public static List<Album> GetAlbumList()
        {
            var albums = new List<Album>();
            var dt = new DataTable();
            var cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(cs))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM Album", connection);
                SqlDataAdapter da = new SqlDataAdapter(command);

                da.Fill(dt);
            }
            foreach (DataRow row in dt.Rows)
            {
                var album = new Album();
                album.Name = row["Name"].ToString();
                album.Directory = row["Directory"].ToString();
                album.Year = row["Year"].ToString();
                albums.Add(album);
            }

            return albums;
        }

        public static bool InsertNewAlbum(Album album)
        {
            var cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            var success = true;
            using (SqlConnection connection = new SqlConnection(cs))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Album (Name, Directory, Year) VALUES (@Name, @Directory, @Year)", connection);
                    cmd.Parameters.Add("@Name", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("@Directory", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("@Year", SqlDbType.VarChar, 50);
                    cmd.Parameters["@Name"].Value = album.Name;
                    cmd.Parameters["@Directory"].Value = album.Directory;
                    cmd.Parameters["@Year"].Value = album.Year;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debugger.Log(9, "Error", ex.ToString());
                    success = false;
                }
            }

            return success;
        }

        public static string MakeValidDirectoryName(string name)
        {
            name = name.Replace(" ", "");
            name = name.Trim();
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }

        public static bool CheckIfAlbumExists(string name)
        {
            var albums = new List<Album>();
            albums = GetAlbumList();

            foreach (var album in albums)
            {
                if (album.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}