using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;

namespace MyStudio.Controllers
{
    // Controllers/PurchaseController.cs
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(ApplicationDbContext context, ILogger<PurchaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Purchase
        public IActionResult Index()
        {
            var purchases = _context.Purchases
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Item)
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            return View(purchases);
        }

        // GET: /Purchase/Create
        public IActionResult Create()
        {
            var model = new CreatePurchaseViewModel
            {
                Vendors = _context.Vendors.Where(v => v.IsActive).ToList(),
                AvailableItems = _context.Items
                    .Include(i => i.Unit)
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.Name)
                    .ToList()
            };

            return View(model);
        }

        // POST: /Purchase/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseViewModel model)
        {
            if (ModelState.IsValid && model.Items.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Generate purchase number
                    var purchaseCount = _context.Purchases.Count(p => p.PurchaseDate.Year == DateTime.Now.Year);
                    var purchaseNumber = $"PUR-{DateTime.Now:yyyy}-{(purchaseCount + 1):D3}";

                    var purchase = new Purchase
                    {
                        PurchaseNumber = purchaseNumber,
                        VendorId = model.VendorId,
                        PurchaseDate = model.PurchaseDate,
                        DeliveryDate = model.DeliveryDate,
                        ShippingCost = model.ShippingCost,
                        Status = "Ordered",
                        Notes = model.Notes,
                        CreatedDate = DateTime.Now
                    };

                    // Add items
                    foreach (var itemModel in model.Items)
                    {
                        var item = await _context.Items.FindAsync(itemModel.ItemId);
                        if (item == null) continue;

                        var lineTotal = itemModel.Quantity * itemModel.UnitPrice *
                                       (1 - itemModel.DiscountPercent / 100);

                        var purchaseItem = new PurchaseItem
                        {
                            ItemId = itemModel.ItemId,
                            Quantity = itemModel.Quantity,
                            UnitPrice = itemModel.UnitPrice,
                            DiscountPercent = itemModel.DiscountPercent,
                            TaxPercent = model.TaxPercent,
                            LineTotal = lineTotal,
                            BatchNumber = itemModel.BatchNumber,
                            ExpiryDate = itemModel.ExpiryDate
                        };

                        purchase.PurchaseItems.Add(purchaseItem);
                        purchase.SubTotal += lineTotal;
                    }

                    // Calculate totals
                    purchase.TaxAmount = purchase.SubTotal * (model.TaxPercent / 100);
                    purchase.TotalAmount = purchase.SubTotal + purchase.TaxAmount + purchase.ShippingCost;

                    _context.Add(purchase);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Purchase order {purchase.PurchaseNumber} created successfully!";
                    return RedirectToAction(nameof(Details), new { id = purchase.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating purchase order");
                    ModelState.AddModelError("", "Error creating purchase order. Please try again.");
                }
            }

            // Reload dropdown data if validation fails
            model.Vendors = _context.Vendors.Where(v => v.IsActive).ToList();
            model.AvailableItems = _context.Items
                .Include(i => i.Unit)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToList();

            return View(model);
        }

        // GET: /Purchase/Details/5
        public IActionResult Details(int id)
        {
            var purchase = _context.Purchases
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Item)
                .ThenInclude(i => i.Unit)
                .FirstOrDefault(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: /Purchase/Receive/5
        public IActionResult Receive(int id)
        {
            var purchase = _context.Purchases
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Item)
                .ThenInclude(i => i.Unit)
                .FirstOrDefault(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: /Purchase/ReceiveItem
        [HttpPost]
        public async Task<IActionResult> ReceiveItem(int purchaseItemId, int quantityReceived, decimal actualUnitPrice)
        {
            var purchaseItem = await _context.PurchaseItems
                .Include(pi => pi.Purchase)
                .Include(pi => pi.Item)
                .FirstOrDefaultAsync(pi => pi.Id == purchaseItemId);

            if (purchaseItem == null)
            {
                return Json(new { success = false, message = "Purchase item not found" });
            }

            if (quantityReceived > purchaseItem.Quantity)
            {
                return Json(new { success = false, message = "Quantity received cannot exceed ordered quantity" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update purchase item
                purchaseItem.QuantityReceived = quantityReceived;
                purchaseItem.ActualUnitPrice = actualUnitPrice;

                // Create stock movement
                var stockMovement = new StockMovement
                {
                    ItemId = purchaseItem.ItemId,
                    MovementDate = DateTime.Now,
                    MovementType = "Purchase",
                    Quantity = quantityReceived,
                    UnitCost = actualUnitPrice,
                    Reference = purchaseItem.Purchase.PurchaseNumber,
                    Notes = $"Purchase receipt - {purchaseItem.Purchase.PurchaseNumber}",
                    CreatedDate = DateTime.Now
                };

                _context.Add(stockMovement);

                // Update purchase status if all items are received
                var allItemsReceived = purchaseItem.Purchase.PurchaseItems
                    .All(pi => pi.QuantityReceived >= pi.Quantity);

                if (allItemsReceived)
                {
                    purchaseItem.Purchase.Status = "Received";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Item received successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error receiving item");
                return Json(new { success = false, message = "Error receiving item" });
            }
        }
    }
}
