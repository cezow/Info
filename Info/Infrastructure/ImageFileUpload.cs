using Info.Infrastructure;
using LazZiya.ImageResize;
using System.Drawing;

namespace Info.Infrastructure
{
    public class ImageFileUpload
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        public ImageFileUpload(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        public FileSendResult SendFile(IFormFile picture, string destination, int width)
        {
            FileSendResult SendFile = new();

            string extension = Path.GetExtension(picture.FileName);
            if (!FileTypeCheck(extension))
            {
                SendFile.Name = Path.GetFileName(picture.FileName);
                SendFile.Success = false;
                SendFile.Error = "Niepoprawny typ pliku graficznego.";
                return SendFile;
            }

            //wygenerowanie nazwy i ustalenie ścieżki docelowej
            SendFile.Name = Guid.NewGuid().ToString() + extension;
            var upload = Path.Combine(hostingEnvironment.WebRootPath, destination);
            var filePath = Path.Combine(upload, SendFile.Name);

            //przesłanie pliku na serwer
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                picture.CopyTo(fileStream);
            }

            //Skalowanie obrazka i zapisanie w podkatalogu
            string path = Path.Combine(filePath);
            using (var imgFile = Image.FromFile(path))
            {
                var miniFile = imgFile.ScaleByWidth(width);
                upload = Path.Combine(hostingEnvironment.WebRootPath, destination + "\\mini");
                filePath = Path.Combine(upload, SendFile.Name);
                miniFile.SaveAs(filePath);
            }

            SendFile.Success = true;
            return SendFile;
        }


        private static bool FileTypeCheck(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".png" or ".gif" => true,
                _ => false,
            };
        }
    }
}