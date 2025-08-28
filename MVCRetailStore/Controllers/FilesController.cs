using Microsoft.AspNetCore.Mvc;
using MVCRetailStore.Models;
using MVCRetailStore.Services;

namespace MVCRetailStore.Controllers
{

    //Reference: Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 4: Mastering Azure File Share!
    // According to Reece Waving (2025), file-related actions in controllers can call service methods for uploading, listing, and downloading files.
    // I used this reftence in FilesController to understand the structure and methods needed to manage files in Azure File Share.

    public class FilesController : Controller
    {
        private readonly AzureFileShareService _fileShareService;

        public FilesController(AzureFileShareService fileShareService)
        {
            _fileShareService = fileShareService;
        }

        public async Task<IActionResult> Index()
        {
            List<FileModel> files;
            try
            {
                files = await _fileShareService.ListFilesAsync("uploads");
                if (TempData["Message"] != null) ViewBag.Message = TempData["Message"];
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Failed to load files: {ex.Message}";
                files = new List<FileModel>();
            }
            return View(files);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }
            try
            {
                using var stream = file.OpenReadStream();
                await _fileShareService.UploadFileAsync("uploads", file.FileName, stream);
                TempData["Message"] = $"File '{file.FileName}' uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"File upload failed: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name cannot be null or empty.");

            try
            {
                var fileStream = await _fileShareService.DownLoadFileAsync("uploads", fileName);
                if (fileStream == null) return NotFound($"File '{fileName}' not found.");
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                return BadRequest($"Error downloading file: {e.Message}");
            }
        }
    }
}
/*
Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage! (Version 2.0) [Source code].
Available at: <https://youtu.be/CuszKqZvRuM?si=lTfJaqI02wmHcIkh>
[Accessed 26 August 2025].


Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 4: Mastering Azure File Share! (Version 2.0) [Source code].
Available at: <https://youtu.be/A-mVVL88oEg?si=sUYFyrY2wQc6Lny0>
[Accessed 26 August 2025].

*/