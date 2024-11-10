using LargeFileUploadSample.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace LargeFileUploadSample.Controllers
{
    public class HomeController : Controller
    {
        private const string UploadPath = "wwwroot/uploads";
        private const string TempFolder = "wwwroot/temp";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            if (!Directory.Exists(UploadPath))
            {
                Directory.CreateDirectory(UploadPath);
            }

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }

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

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm]FileUploadViewModel viewModel) {

            if (viewModel.FileChunk != null && viewModel.FileChunk.Length > 0)
            {
                var tempFilePath = Path.Combine(TempFolder, viewModel.Index.ToString());
                
                // Save the chunk temporarily
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await viewModel.FileChunk.CopyToAsync(stream);
                }

                // Check if all chunks are uploaded
                //if (Directory.GetFiles(TempFolder).Length == viewModel.TotalChunks)
                //{
                //    var finalFilePath = Path.Combine(UploadPath, "finalfile");
                //    using (var finalStream = new FileStream(finalFilePath, FileMode.Create))
                //    {
                //        for (int i = 0; i < viewModel.TotalChunks; i++)
                //        {
                //            var chunkPath = Path.Combine(TempFolder, i.ToString());
                //            var chunkData = await System.IO.File.ReadAllBytesAsync(chunkPath);
                //            await finalStream.WriteAsync(chunkData);
                //            System.IO.File.Delete(chunkPath); // Remove chunk after appending
                //        }
                //    }

                //    return Json(new { message = "File upload complete" });
                //}

                return Json(new { message = "Chunk uploaded successfully" });
            }

            return Json(new { message = "Invalid file chunk" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class FileUploadViewModel {
       public IFormFile FileChunk { get; set; }
       public int Index { get; set; }
       public int TotalChunks { get; set; }
    }
}
