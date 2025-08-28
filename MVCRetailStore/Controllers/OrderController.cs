using Microsoft.AspNetCore.Mvc;
using MVCRetailStore.Models;
using MVCRetailStore.Services;

namespace MVCRetailStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrderController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Customers"] = await _tableStorageService.GetAllCustomersAsync();
            ViewData["Products"] = await _tableStorageService.GetAllProductsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Customers"] = await _tableStorageService.GetAllCustomersAsync();
                ViewData["Products"] = await _tableStorageService.GetAllProductsAsync();
                return View(order);
            }
            order.PartitionKey = "OrderPartition";
            order.RowKey = Guid.NewGuid().ToString();
            order.orderStatus = "New"; 
            order.orderDate = DateTime.UtcNow;
            var product = (await _tableStorageService.GetAllProductsAsync())
                          .FirstOrDefault(p => p.RowKey == order.productId);
            if (product != null)
                order.orderTotal = product.Price * Math.Max(1, order.orderQuantity);

            await _tableStorageService.AddOrderAsync(order);
            string message = $"NewOrder|OrderRowKey:{order.RowKey}|CustomerRowKey:{order.customerId}|ProductRowKey:{order.productId}|Qty:{order.orderQuantity}|Date:{order.orderDate:O}";
            await _queueService.SendMessage(message);

            TempData["Success"] = "Order created and queued.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);
            if (order == null) return NotFound();

            ViewData["Customers"] = await _tableStorageService.GetAllCustomersAsync();
            ViewData["Products"] = await _tableStorageService.GetAllProductsAsync();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Customers"] = await _tableStorageService.GetAllCustomersAsync();
                ViewData["Products"] = await _tableStorageService.GetAllProductsAsync();
                return View(order);
            }
            // Reference: Stack Overflow. 2015. MVC - Data Calculations Best Practice - ViewModel vs. Controller.
            // According to Stack Overflow contributors (2015), calculations should be handled in a separate business layer class,
            // I used this method to calculate the total of the quantity timed by the price of the product to give the total outcome of the order


            var product = (await _tableStorageService.GetAllProductsAsync())
                          .FirstOrDefault(p => p.RowKey == order.productId);
            if (product != null)
                order.orderTotal = product.Price * Math.Max(1, order.orderQuantity);

            order.orderDate = DateTime.SpecifyKind(order.orderDate, DateTimeKind.Utc);

            await _tableStorageService.UpdateOrderAsync(order);

            TempData["Success"] = "Order updated.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);
            TempData["Success"] = "Order deleted.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}


/*
Reece Waving. 2025.CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1 (Version 2.0) [Source code].
Available at: <https://youtu.be/Txp7VYUMBGQ?si=rK_KNE690UyMCi3O>
[Accessed 25 August 2025].


Reece Waving. 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 3: Never Lose Data Again with Queue Storage!(Version 2.0) [Source code].
Available at: <https://youtu.be/VbZ3Pi63yEc?si=LQjhLWhylEcbOl7z>
[Accessed 28 August 2025].

/*
Stack Overflow. 2015. MVC - Data Calculations Best Practice - ViewModel vs. Controller. [online]
Available at: <https://stackoverflow.com/questions/31730642/mvc-data-calculations-best-practice-viewmodel-vs-controller>
[Accessed 28 August 2025].

*/