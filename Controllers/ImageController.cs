using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using ImageResizer.Models;
using ImageResizer.Services;

namespace ImageResizer.Controllers
{
    // ==========================================================
    //  IMAGE CONTROLLER
    //  Handles image upload, resizing requests, and returns
    //  results for display in the web interface. Uses the
    //  ImageResizeService to perform processing.
    // ==========================================================
    public class ImageController : Controller
    {
        private readonly ImageResizeService _resizeService;
        private readonly IWebHostEnvironment _env;

        // ======================================================
        //  CONSTRUCTOR
        //  Injects the image resize service and web host environment
        //  (used to determine file storage paths in wwwroot).
        // ======================================================
        public ImageController(ImageResizeService resizeService, IWebHostEnvironment env)
        {
            _resizeService = resizeService;
            _env = env;
        }

        // ======================================================
        //  GET: /Image/Index
        //  Displays the upload and resize form.
        // ======================================================
        [HttpGet]
        public IActionResult Index() => View(new ImageResultViewModel());

        // ======================================================
        //  POST: /Image/Resize
        //  Handles image upload, performs resizing via the service,
        //  and returns timing and result data to the view.
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Resize(IFormFile imageFile, int targetSize = 500, float maskOutPercent = 0.0f)
        {
            var model = new ImageResultViewModel();

            // --------------------------------------------------
            // Validate uploaded file
            // --------------------------------------------------
            if (imageFile == null || imageFile.Length == 0)
            {
                model.Error = "Please select a valid image file.";
                return View("Index", model);
            }

            // --------------------------------------------------
            // Create upload directory inside wwwroot/resized
            // --------------------------------------------------
            string uploads = Path.Combine(_env.WebRootPath, "resized");
            Directory.CreateDirectory(uploads);

            // Save uploaded image to disk
            string srcPath = Path.Combine(uploads, Path.GetFileName(imageFile.FileName));
            using (var fs = new FileStream(srcPath, FileMode.Create))
                await imageFile.CopyToAsync(fs);

            // --------------------------------------------------
            // Perform resize operation (both pixel & bilinear)
            // --------------------------------------------------
            var result = _resizeService.ProcessImage(srcPath, targetSize, maskOutPercent);

            // --------------------------------------------------
            // Map results to the view model
            // --------------------------------------------------
            model.SourceImage = Path.GetFileName(srcPath);
            model.PixelImage = Path.GetFileName(result.PixelImagePath);
            model.BilinearImage = Path.GetFileName(result.BilinearImagePath);
            model.TimePixel = result.PixelTime;
            model.TimeBilinear = result.BilinearTime;
            model.Faster = result.FasterMessage;

            model.SourceWidth = result.SourceWidth;
            model.SourceHeight = result.SourceHeight;
            model.PixelWidth = result.PixelWidth;
            model.PixelHeight = result.PixelHeight;
            model.BilinearWidth = result.BilinearWidth;
            model.BilinearHeight = result.BilinearHeight;

            // --------------------------------------------------
            // Return the results to the view
            // --------------------------------------------------
            return View("Index", model);
        }
    }
}
