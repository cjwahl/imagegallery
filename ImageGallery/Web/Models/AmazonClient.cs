using Amazon.S3;

namespace ImageGallery.Models
{
    public class AmazonClient
    {
        public AmazonClient()
        {
            S3Client = new AmazonS3Client("***", "***", Amazon.RegionEndpoint.USWest2);
            BucketName = "cjwahldotcom-image-storage";
        }

        public IAmazonS3 S3Client { get; set; }
        public string BucketName { get; set; }
    }
}