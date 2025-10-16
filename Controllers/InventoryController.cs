using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;

namespace MyStudio.Controllers
{
    // Controllers/InventoryController.cs
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Inventory
        public IActionResult Index()
        {
            // Get all active items
            var items = _context.Items
                .Include(i => i.Unit)
                .Include(i => i.ItemGroup)
                .Include(i => i.StockMovements) // Include stock movements for calculation
                .Where(i => i.IsActive)
                .ToList();

            // Calculate current stock for each item (client-side)
            foreach (var item in items)
            {
                // Calculate current stock from stock movements
                item.StockMovements = _context.StockMovements
                    .Where(sm => sm.ItemId == item.Id)
                    .ToList();
            }

            // Get low stock items (calculated client-side)
            var lowStockItems = items
                .Where(i => i.CurrentStock <= i.ReorderLevel)
                .ToList();

            var model = new InventoryReportViewModel
            {
                Items = items,
                LowStockItems = lowStockItems,
                RecentPurchases = _context.Purchases
                    .Include(p => p.Vendor)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        // GET: /Inventory/Items
        public IActionResult Items()
        {
            var items = _context.Items
                .Include(i => i.Unit)
                .Include(i => i.ItemGroup)
                .Include(i => i.ItemSubGroup)
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToList();

            return View(items);
        }

        // GET: /Inventory/CreateItem
        public IActionResult CreateItem()
        {
            var model = new CreateItemViewModel
            {
                Units = _context.Units.Where(u => u.IsActive).ToList(),
                ItemGroups = _context.ItemGroups.Where(ig => ig.IsActive).ToList(),
                ItemSubGroups = _context.ItemSubGroups.Where(isg => isg.IsActive).ToList()
            };

            return View(model);
        }

        // POST: /Inventory/CreateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if SKU already exists
                if (_context.Items.Any(i => i.SKU == model.SKU))
                {
                    ModelState.AddModelError("SKU", "SKU already exists");
                    model.Units = _context.Units.Where(u => u.IsActive).ToList();
                    model.ItemGroups = _context.ItemGroups.Where(ig => ig.IsActive).ToList();
                    model.ItemSubGroups = _context.ItemSubGroups.Where(isg => isg.IsActive).ToList();
                    return View(model);
                }

                var item = new Item
                {
                    SKU = model.SKU,
                    Name = model.Name,
                    Description = model.Description,
                    Brand = model.Brand,
                    Model = model.Model,
                    UnitId = model.UnitId,
                    ItemGroupId = model.ItemGroupId,
                    ItemSubGroupId = model.ItemSubGroupId,
                    MinimumStock = model.MinimumStock,
                    MaximumStock = model.MaximumStock,
                    ReorderLevel = model.ReorderLevel,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Add(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Item '{item.Name}' created successfully!";
                return RedirectToAction(nameof(Items));
            }

            // Reload dropdown data if validation fails
            model.Units = _context.Units.Where(u => u.IsActive).ToList();
            model.ItemGroups = _context.ItemGroups.Where(ig => ig.IsActive).ToList();
            model.ItemSubGroups = _context.ItemSubGroups.Where(isg => isg.IsActive).ToList();
            return View(model);
        }

        // GET: /Inventory/ItemDetails/5
        public IActionResult ItemDetails(int id)
        {
            var item = _context.Items
                .Include(i => i.Unit)
                .Include(i => i.ItemGroup)
                .Include(i => i.ItemSubGroup)
                .Include(i => i.StockMovements)
                .FirstOrDefault(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: /Inventory/ItemGroups
        public IActionResult ItemGroups()
        {
            var groups = _context.ItemGroups
                .Include(ig => ig.SubGroups)
                .Where(ig => ig.IsActive)
                .OrderBy(ig => ig.Name)
                .ToList();

            return View(groups);
        }

        // POST: /Inventory/CreateItemGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItemGroup(string name, string code, string? description)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "Name and Code are required";
                return RedirectToAction(nameof(ItemGroups));
            }

            if (_context.ItemGroups.Any(ig => ig.Code == code))
            {
                TempData["ErrorMessage"] = "Group code already exists";
                return RedirectToAction(nameof(ItemGroups));
            }

            var group = new ItemGroup
            {
                Name = name,
                Code = code,
                Description = description,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Add(group);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Item group '{group.Name}' created successfully!";
            return RedirectToAction(nameof(ItemGroups));
        }
    }
}
