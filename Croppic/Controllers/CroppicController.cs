using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using System.Web.Mvc;

//Coding by Haneef puttur

namespace Croppic.Controllers
    {
    public class CroppicController : Controller
        {

        public ActionResult Index()
            {
            return View();
            }

        [HttpPost]
        public string UploadOriginalImage(HttpPostedFileBase img)
            {
            string folder = Server.MapPath("~/Temp");
            string extension = Path.GetExtension(img.FileName);
            string pic = System.IO.Path.GetFileName(Guid.NewGuid().ToString());
            var tempPath = Path.ChangeExtension(pic, extension);
            string tempFilePath = System.IO.Path.Combine(folder, tempPath);
            img.SaveAs(tempFilePath);
            var image = System.Drawing.Image.FromFile(tempFilePath);
            var result = new
            {
                status = "success",
                width = image.Width,
                height = image.Height,
                url = "../Temp/" + tempPath
            };
            return JsonConvert.SerializeObject(result);
            }


        [HttpPost]
        public string CroppedImage(string imgUrl, int imgInitW, int imgInitH, double imgW, double imgH, int imgY1, int imgX1, int cropH, int cropW, int rotation)
            {
            var originalFilePath = Server.MapPath(imgUrl);
            var fileName = CropImage(originalFilePath, imgInitW, imgInitH, (int)imgW, (int)imgH, imgY1, imgX1, cropH, cropW, rotation);


            //unlock any pending procss , close any open files and delete temp file
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            if (System.IO.File.Exists(originalFilePath))
                {
                System.IO.File.Delete(originalFilePath);

                }

            var result = new
            {
                status = "success",
                url = "../Cropped/" + fileName
            };

            return JsonConvert.SerializeObject(result);
            }

        private string CropImage(string originalFilePath, int origW, int origH, int targetW, int targetH, int cropStartY, int cropStartX, int cropH, int cropW, int rotation)
            {
            var originalImage = Image.FromFile(originalFilePath);

            //if rotation is required call rotation function
            if (rotation != 0)
                {
                originalImage = RotateImage(originalImage, rotation);
                }

            var resizedOriginalImage = new Bitmap(originalImage, targetW, targetH);
            var targetImage = new Bitmap(cropW, cropH);

            //create brush to fill image background with white color
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255));

            using (Graphics g = Graphics.FromImage(targetImage))
                {
                //fill bg with white color
                g.FillRectangle(brush, 0, 0, cropW, cropH);

                g.DrawImage(resizedOriginalImage, new Rectangle(0, 0, cropW, cropH), new Rectangle(cropStartX, cropStartY, cropW, cropH), GraphicsUnit.Pixel);
                g.Dispose();
                }


            // put new name for the file
            string newname = System.IO.Path.GetFileName(Guid.NewGuid().ToString());
            // change the extesion of file to jpg
            string ext = ".jpg";
            //build the file name
            string fileName = Path.ChangeExtension(newname, ext);
            //move the file to cropped folder      
            var folder = Server.MapPath("~/Cropped");
            string croppedPath = Path.Combine(folder, fileName);
            //save image and force to format conversion to jpg
            targetImage.Save(croppedPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            targetImage.Dispose();
            originalImage.Dispose();
            return fileName;
            }

        public static Bitmap RotateImage(Image img, float rotationAngle)
            {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            gfx.RotateTransform(rotationAngle);
            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gfx.DrawImage(img, new Point(0, 0));
            gfx.Dispose();
            return bmp;
            }
        }
    }