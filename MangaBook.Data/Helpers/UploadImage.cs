using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace MangaBook.Data.Helpers
{
    public class UploadImage
    {

        public static string UploadImageFile(IFormFile files)
        {
            if (files != null)
            {
                if (files.Length > 0)
                {
                    var fileName = Path.GetFileName(files.FileName);
                    var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                    var fileExtension = Path.GetExtension(fileName);
                    var newFileName = String.Concat(myUniqueFileName, fileExtension);

                    var filepath =
            new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")).Root + $@"\{newFileName}";

                    using (FileStream fs = System.IO.File.Create(filepath))
                    {
                        files.CopyTo(fs);
                        fs.Flush();
                    }


                    var newImageName = newFileName;

                    return newImageName.ToString();
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }

        }
    }
}
