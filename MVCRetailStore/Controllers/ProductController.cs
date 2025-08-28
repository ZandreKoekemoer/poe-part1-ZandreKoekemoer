using Microsoft.AspNetCore.Mvc;
using MVCRetailStore.Models;
using MVCRetailStore.Services;

namespace MVCRetailStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductController(BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile file)
        {
            if (!ModelState.IsValid)
                return View(product);

            //Reference: Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage!.
            // According to Reece Waving (2025), image files can be uploaded to Azure Blob Storage by opening a file stream and sending it via the BlobService client.
            // I used this reference in the ProductController to handle product image uploads for my create view
            try
            {
                if (file != null && file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    product.ImageUrl = await _blobService.UploadsAsync(stream, file.FileName);
                }

                product.Price = Math.Round(product.Price, 2);
                product.PartitionKey = "ProductPartition";
                product.RowKey = Guid.NewGuid().ToString();

                await _tableStorageService.AddProductAsync(product);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? file)
        {
            if (!ModelState.IsValid)
                return View(product);

            //Reference: Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage.
            // According to Reece Waving (2025), image files can be uploaded to Azure Blob Storage by opening a file stream and sending it via the BlobService client.
            // I used this reference to use for when editing the file upload by replacing with another image.
            try
            {
                if (file != null && file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    product.ImageUrl = await _blobService.UploadsAsync(stream, file.FileName);
                }

                product.Price = Math.Round(product.Price, 2);
                await _tableStorageService.UpdateProductAsync(product);

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, string? imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                    await _blobService.DeleteBlobAsync(imageUrl);

                await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}


/*
Reece Waving. 2025.CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1 (Version 2.0) [Source code].
Available at: <https://youtu.be/Txp7VYUMBGQ?si=rK_KNE690UyMCi3O>
[Accessed 25 August 2025].

Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage! (Version 2.0) [Source code].
Available at: <https://youtu.be/CuszKqZvRuM?si=lTfJaqI02wmHcIkh>
[Accessed 26 August 2025].
*/