using Microsoft.AspNetCore.Mvc;
using MVCRetailStore.Models;
using MVCRetailStore.Services;

namespace MVCRetailStore.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomerController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            customer.PartitionKey = "CustomerPartition";
            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddCustomerAsync(customer);
            TempData["Success"] = "Customer created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            await _tableStorageService.UpdateCustomerAsync(customer);
            TempData["Success"] = "Customer updated.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            TempData["Success"] = "Customer deleted.";
            return RedirectToAction("Index");
        }
    }
}

/*
Reece Waving. 2025.CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1 (Version 2.0) [Source code].
Available at: <https://youtu.be/Txp7VYUMBGQ?si=rK_KNE690UyMCi3O>
[Accessed 25 August 2025].
*/