using ImageProcessing.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO;
using Image = SixLabors.ImageSharp.Image;

namespace ImageProcessing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile ImageUrl, int maxWidth, int maxHeight, bool grayscale, int blurSigma, int brightness, int contrast)
        {
            byte[] resizedImage, filteredImage, brightedImage;
            try
            {
                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                // Ensure the directory exists; create it if not
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                // Generate a unique file name to avoid overwriting existing files
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUrl.FileName);

                // Combine the directory and file name to get the full path
                var filePath = Path.Combine(uploadDirectory, fileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageUrl.CopyToAsync(stream);
                }

                // Resize the Image 
                byte[] processedImage = await ProcessImageAsync(ImageUrl.OpenReadStream(), maxWidth, maxHeight, grayscale, blurSigma, brightness, contrast);

                using (var image = Image.Load(ImageUrl.OpenReadStream()))
                {
                    await ResizeImage(image, maxWidth, maxHeight);
                    //await ApplyFilters(image, grayscale, blurSigma);
                    //await AdjustBrightnessContrast(image, brightness, contrast);

                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, new JpegEncoder());
                        resizedImage =  outputStream.ToArray();
                    }
                }
                string base64Image1 = Convert.ToBase64String(resizedImage);
                ViewData["ResizedImage"] = base64Image1;

                using (var image = Image.Load(ImageUrl.OpenReadStream()))
                {
                    await ApplyFilters(image, grayscale, blurSigma);
                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, new JpegEncoder());
                        filteredImage = outputStream.ToArray();
                    }
                }
                string base64Image2 = Convert.ToBase64String(filteredImage);
                ViewData["filteredImage"] = base64Image2;

                using (var image = Image.Load(ImageUrl.OpenReadStream()))
                {
                    await AdjustBrightnessContrast(image, brightness, contrast);
                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, new JpegEncoder());
                        brightedImage = outputStream.ToArray();
                    }
                }
                string base64Image3 = Convert.ToBase64String(brightedImage);
                ViewData["brightedImage"] = base64Image3;


                // Pass the base64Image directly to the view.
                return View("Upload");
                /*ViewData["ProcessedImage"] = processedImage;
                return View("Upload");*/

                // Rest of your code...
            }
            catch (Exception ex)
            {
                
                return View("Index");
            }

            
        }

        public async Task<byte[]> ProcessImageAsync(Stream stream, int maxWidth, int maxHeight, bool grayscale, float blurSigma, float brightness, float contrast)
        {
            using (var image = Image.Load(stream))
            {
                await ResizeImage(image, maxWidth, maxHeight);
                //await ApplyFilters(image, grayscale, blurSigma);
                //await AdjustBrightnessContrast(image, brightness, contrast);

                using (var outputStream = new MemoryStream())
                {
                    image.Save(outputStream, new JpegEncoder());
                    return outputStream.ToArray();
                }
            }
        }

        private async Task ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            maxWidth = image.Width/2;
            maxHeight = image.Height/2;
            if (maxWidth > 0 && maxHeight > 0)
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));
            }
        }

        private async Task ApplyFilters(Image image, bool grayscale, float blurSigma)
        {
            if (grayscale)
            {
                image.Mutate(x => x.Grayscale());
            }

            if (blurSigma > 0)
            {
                image.Mutate(x => x.GaussianBlur(blurSigma));
            }
        }

        private async Task AdjustBrightnessContrast(Image image, float brightness, float contrast)
        {
            if (brightness != 0 || contrast != 0)
            {
                image.Mutate(x => x
                    .Brightness(brightness)
                    .Contrast(contrast));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}