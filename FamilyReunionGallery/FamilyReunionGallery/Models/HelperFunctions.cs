using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace FamilyReunionGallery.Models
{
    public static class HelperFunctions
    {
        private static string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        public static Bitmap ResizeImage(Image image, int width, int height)
        {

            
            int orientationId = 0x0112;
            var property = image.GetPropertyItem(orientationId);
            var fType = RotateFlipType.RotateNoneFlipNone;
            if (image.PropertyIdList.Contains(orientationId))
            {
                var pItem = image.GetPropertyItem(orientationId);
                fType = GetRotateFlipTypeByExifOrientationData(pItem.Value[0]);
                if (fType != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(fType);
                    image.RemovePropertyItem(orientationId); 
                }
            }


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

        public static DataTable GetSqlData()
        {
            var dt = new DataTable();

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
                album.AlbumTitle = row["Name"].ToString();
                album.Directory = row["Directory"].ToString();
                album.Year = row["Year"].ToString();
                album = GetAlbumImages(album);
                albums.Add(album);
            }
            foreach(var album in albums)
            {
                if(string.IsNullOrEmpty(album.Year))
                {
                    album.Year = "2017";
                }
            }
            return albums;
        }

        public static bool InsertNewAlbum(Album album)
        {
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
                    cmd.Parameters["@Name"].Value = album.AlbumTitle;
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
                if (album.Directory == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckIfImageExists(string name)
        {
            var images = GetAllImages();
            foreach (var image in images)
            {
                if (image.Key.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<S3Object> GetAllImages()
        {
            var objectList = new List<S3Object>();
            var client = new AmazonClient();
            using (client.S3Client)
            {
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = client.BucketName,
                };
                ListObjectsResponse response = client.S3Client.ListObjects(request);

                foreach (S3Object entry in response.S3Objects)
                {
                    objectList.Add(entry);
                }
            }
            return objectList;
        }

        public static Album GetAlbumImages(Album album)
        {
            var client = new AmazonClient();
            var objectList = new List<S3Object>();

            using (client.S3Client)
            {
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = client.BucketName,
                };
                ListObjectsResponse response = client.S3Client.ListObjects(request);

                foreach (S3Object entry in response.S3Objects)
                {
                    objectList.Add(entry);
                }
            }
            foreach (S3Object image in objectList)
            {
                if (image.Key.Contains(album.Directory) && image.Size > 0)
                {
                    if (image.Key.Contains("fulls"))
                    {
                        album.FullImages.Add(image);
                    }
                    else if (image.Key.Contains("thumbs"))
                    {
                        album.ThumbImages.Add(image);
                    }
                }
                if (image.Key.Contains("Dashboard") && image.Key.Contains(album.Directory))
                {
                    album.DirectoryImage = image;
                }
            }
            return album;
        }

        public static void UploadImage(HttpPostedFileBase file, DirectoryInfo directory, string directoryName, string name)
        {
            if (file != null && file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                
                var img = Image.FromStream(file.InputStream, true, true);

                if (img.Width > img.Height)
                {
                    if (directory.ToString().Contains("thumbs") || directoryName == "Dashboard")
                    {
                        img = ResizeImage(img, 642, 482);
                    }
                    else
                    {
                        img = ResizeImage(img, 1024, 768);
                    }
                }
                else
                {
                    if (directory.ToString().Contains("thumbs") || directoryName == "Dashboard")
                    {
                        img = ResizeImage(img, 482, 642);
                    }
                    else
                    {
                        img = ResizeImage(img, 768, 1024);
                    }
                }
                
                string s3Key = "";
                if (directory.ToString().Contains("thumbs"))
                {
                    s3Key = string.Format("{0}/thumbs/{1}", directoryName, name);
                }
                else if (directory.ToString().Contains("fulls"))
                {
                    s3Key = string.Format("{0}/fulls/{1}", directoryName, name);
                }
                else
                {
                    s3Key = string.Format("{0}/{1}", directoryName, name);
                }

                UploadS3Image(img, s3Key);
            }
        }

        public static void UploadS3Image(Image img, string s3Key)
        {
            string objectKey = s3Key;
            var client = new AmazonClient();
            using (client.S3Client)
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = client.BucketName,
                    Key = objectKey,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.Now.AddMinutes(5)
                };

                string url = client.S3Client.GetPreSignedURL(request);

                HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
                httpRequest.Method = "PUT";
                using (Stream dataStream = httpRequest.GetRequestStream())
                {
                    byte[] buffer = new byte[8000];
                    using (Stream image = ToStream(img, ImageFormat.Jpeg))
                    {
                        int bytesRead = 0;
                        while ((bytesRead = image.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            dataStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse;
            }
        }

        public static Stream ToStream(this Image image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public static void CreateS3Folder(string folderName)
        {
            var client = new AmazonClient();
            using (client.S3Client)
            {
                var request = new PutObjectRequest();

                request.BucketName = client.BucketName;
                request.Key = string.Concat(folderName, "/");
                request.InputStream = new MemoryStream();
                client.S3Client.PutObject(request);
            }
            client = new AmazonClient();
            using (client.S3Client)
            {
                var request = new PutObjectRequest();

                request.BucketName = client.BucketName;
                request.Key = string.Concat(folderName, "/fulls/");
                request.InputStream = new MemoryStream();
                client.S3Client.PutObject(request);

                request = new PutObjectRequest();

                request.BucketName = client.BucketName;
                request.Key = string.Concat(folderName, "/thumbs/");
                request.InputStream = new MemoryStream();
                client.S3Client.PutObject(request);
            }
        }

        public static void DeleteImage(string key)
        {
            var client = new AmazonClient();

            using (client.S3Client)
            {
                DeleteObjectRequest request = new DeleteObjectRequest();

                request.BucketName = client.BucketName;
                request.Key = key;

                client.S3Client.DeleteObject(request);

                request = new DeleteObjectRequest();

                request.BucketName = client.BucketName;
                request.Key = key.Replace("thumbs", "fulls");

                client.S3Client.DeleteObject(request);
            }

        }

        public static RotateFlipType GetRotateFlipTypeByExifOrientationData(int orientation)
        {
            switch (orientation)
            {
                case 1:
                default:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
            }
        }
    }
}