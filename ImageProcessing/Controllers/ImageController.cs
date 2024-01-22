using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Controllers
{
    public class ImageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }
    }
}
