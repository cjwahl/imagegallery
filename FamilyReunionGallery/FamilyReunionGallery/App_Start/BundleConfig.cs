using System.Web;
using System.Web.Optimization;

namespace FamilyReunionGallery
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                      "~/Scripts/jquery.min.js",
                      "~/Scripts/jquery.poptrox.min.js",
                      "~/Scripts/skel.min.js",
                      "~/Scripts/util.js",
                      "~/Scripts/main.js"));

            bundles.Add(new ScriptBundle("~/bundles/dashboard").Include(
                      "~/Scripts/jquery.min.js",
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/creative.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/upload").Include(
                     "~/Scripts/jquery.min.js",
                     "~/Scripts/dropzone/dropzone.js",
                     "~/Scripts/bootstrap.min.js",
                      "~/Scripts/creative.min.js"));

            bundles.Add(new StyleBundle("~/Content/uploadcss").Include(
                     "~/Scripts/dropzone/basic.css",
                     "~/Scripts/dropzone/dropzone.css",
                     "~/Content/css/bootstrap.css",
                      "~/Content/css/creative.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/css/main.css"));

            bundles.Add(new StyleBundle("~/Content/creative").Include(
                      "~/Content/css/bootstrap.css",
                      "~/Content/css/creative.css"));
        }
    }
}
