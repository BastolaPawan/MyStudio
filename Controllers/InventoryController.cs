using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
        public async Task<IActionResult> CreateItem(CreateItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if SKU already exists
                if (_context.Items.Any(i => i.SKU == viewModel.SKU))
                {
                    ModelState.AddModelError("SKU", "SKU already exists");
                    viewModel.Units = _context.Units.Where(u => u.IsActive).ToList();
                    viewModel.ItemGroups = _context.ItemGroups.Where(ig => ig.IsActive).ToList();
                    viewModel.ItemSubGroups = _context.ItemSubGroups.Where(isg => isg.IsActive).ToList();
                    return View(viewModel);
                }

                var item = new Item
                {
                    SKU = viewModel.SKU,
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    UnitId = viewModel.UnitId,
                    ItemGroupId = viewModel.ItemGroupId,
                    ItemSubGroupId = viewModel.ItemSubGroupId,
                    MinimumStock = viewModel.MinimumStock,
                    MaximumStock = viewModel.MaximumStock,
                    ReorderLevel = viewModel.ReorderLevel,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Add(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Item '{item.Name}' created successfully!";
                return RedirectToAction(nameof(Items));
            }

            // Reload dropdown data if validation fails
            viewModel.Units = _context.Units.Where(u => u.IsActive).ToList();
            viewModel.ItemGroups = _context.ItemGroups.Where(ig => ig.IsActive).ToList();
            viewModel.ItemSubGroups = _context.ItemSubGroups.Where(isg => isg.IsActive).ToList();
            return View(viewModel);
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



        #region Vendor region
        // In InventoryController.cs or VendorController.cs

        // GET: /Inventory/Vendors
        public IActionResult Vendors()
        {
            var vendors = _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToList();

            return View(vendors);
        }

        // GET: /Inventory/CreateVendor
        public IActionResult CreateVendor()
        {
            return View();
        }

        // POST: /Inventory/CreateVendor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVendor(CreateVendorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if vendor name already exists
                    if (_context.Vendors.Any(v => v.Name == model.Name))
                    {
                        ModelState.AddModelError("Name", "Vendor with this name already exists");
                        return View(model);
                    }

                    var vendor = new Vendor
                    {
                        Name = model.Name,
                        ContactPerson = model.ContactPerson,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        TaxNumber = model.TaxNumber,
                        PaymentTerms = model.PaymentTerms,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.Vendors.Add(vendor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Vendor '{vendor.Name}' created successfully!";
                    return RedirectToAction(nameof(Vendors));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vendor");
                    ModelState.AddModelError("", "An error occurred while creating the vendor. Please try again.");
                }
            }

            return View(model);
        }

        // GET: /Inventory/EditVendor/5
        public IActionResult EditVendor(int id)
        {
            var vendor = _context.Vendors.Find(id);
            if (vendor == null)
            {
                return NotFound();
            }

            var model = new CreateVendorViewModel
            {
                Name = vendor.Name,
                ContactPerson = vendor.ContactPerson,
                Email = vendor.Email,
                Phone = vendor.Phone,
                Address = vendor.Address,
                TaxNumber = vendor.TaxNumber,
                PaymentTerms = vendor.PaymentTerms
            };

            return View(model);
        }

        // POST: /Inventory/EditVendor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVendor(int id, CreateVendorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vendor = await _context.Vendors.FindAsync(id);
                    if (vendor == null)
                    {
                        return NotFound();
                    }

                    // Check if vendor name already exists (excluding current vendor)
                    if (_context.Vendors.Any(v => v.Name == model.Name && v.Id != id))
                    {
                        ModelState.AddModelError("Name", "Vendor with this name already exists");
                        return View(model);
                    }

                    vendor.Name = model.Name;
                    vendor.ContactPerson = model.ContactPerson;
                    vendor.Email = model.Email;
                    vendor.Phone = model.Phone;
                    vendor.Address = model.Address;
                    vendor.TaxNumber = model.TaxNumber;
                    vendor.PaymentTerms = model.PaymentTerms;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Vendor '{vendor.Name}' updated successfully!";
                    return RedirectToAction(nameof(Vendors));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vendor");
                    ModelState.AddModelError("", "An error occurred while updating the vendor. Please try again.");
                }
            }

            return View(model);
        }

        // POST: /Inventory/DeleteVendor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            try
            {
                var vendor = await _context.Vendors.FindAsync(id);
                if (vendor == null)
                {
                    return NotFound();
                }

                // Check if vendor has any purchases
                bool hasPurchases = _context.Purchases.Any(p => p.VendorId == id);
                if (hasPurchases)
                {
                    TempData["ErrorMessage"] = "Cannot delete vendor that has associated purchases. You can deactivate them instead.";
                    return RedirectToAction(nameof(Vendors));
                }

                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Vendor '{vendor.Name}' deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vendor");
                TempData["ErrorMessage"] = "An error occurred while deleting the vendor. Please try again.";
            }

            return RedirectToAction(nameof(Vendors));
        }

        // POST: /Inventory/ToggleVendorStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleVendorStatus(int id)
        {
            try
            {
                var vendor = await _context.Vendors.FindAsync(id);
                if (vendor == null)
                {
                    return Json(new { success = false, message = "Vendor not found" });
                }

                vendor.IsActive = !vendor.IsActive;
                await _context.SaveChangesAsync();

                var status = vendor.IsActive ? "activated" : "deactivated";
                return Json(new { success = true, message = $"Vendor {status} successfully", isActive = vendor.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling vendor status");
                return Json(new { success = false, message = "Error updating vendor status" });
            }
        }
        #endregion

        #region ImportVendors from excel sheets
        // GET: /Inventory/ImportVendors
        public IActionResult ImportVendors()
        {
            return View();
        }

        // POST: /Inventory/ImportVendors
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportVendors(ImportVendorsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ExcelFile == null || model.ExcelFile.Length == 0)
            {
                ModelState.AddModelError("ExcelFile", "Please select an Excel file");
                return View(model);
            }

            // Validate file type
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(model.ExcelFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ExcelFile", "Please upload a valid Excel file (.xlsx or .xls)");
                return View(model);
            }

            try
            {
                var result = await ProcessExcelFile(model.ExcelFile, model.UpdateExisting, model.SkipErrors);

                // Store result in TempData to display on results page
                TempData["ImportResult"] = JsonSerializer.Serialize(result);

                return RedirectToAction(nameof(ImportResults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vendors from Excel");
                ModelState.AddModelError("", $"Error importing file: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Inventory/ImportResults
        public IActionResult ImportResults()
        {
            if (TempData["ImportResult"] is not string resultJson)
            {
                return RedirectToAction(nameof(ImportVendors));
            }

            var result = JsonSerializer.Deserialize<ImportResultViewModel>(resultJson);
            return View(result);
        }

        // GET: /Inventory/DownloadVendorTemplate
        public IActionResult DownloadVendorTemplate()
        {

            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Vendor Template");

                // Headers
                var headers = new[] { "Name", "ContactPerson", "Email", "Phone", "Address", "TaxNumber", "PaymentTerms" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Sample data for photo studio vendors
                var sampleVendors = new[]
                {
            new { Name = "Camera World Ltd.", ContactPerson = "John Smith", Email = "john@cameraworld.com", Phone = "+1-555-0101", Address = "123 Camera Street, Photo City", TaxNumber = "TXN-001", PaymentTerms = "Net 30" },
            new { Name = "Lighting Solutions Inc.", ContactPerson = "Sarah Johnson", Email = "sarah@lightingsol.com", Phone = "+1-555-0102", Address = "456 Light Avenue, Studio Town", TaxNumber = "TXN-002", PaymentTerms = "50% Advance" },
            new { Name = "Paper Supply Co.", ContactPerson = "Mike Davis", Email = "mike@papersupply.co", Phone = "+1-555-0103", Address = "789 Paper Road, Printville", TaxNumber = "TXN-003", PaymentTerms = "Net 15" },
            new { Name = "Backdrop Masters", ContactPerson = "Emily Wilson", Email = "emily@backdropmasters.com", Phone = "+1-555-0104", Address = "321 Background Lane, Studio City", TaxNumber = "TXN-004", PaymentTerms = "Net 30" }
        };

                // Add sample data
                for (int row = 0; row < sampleVendors.Length; row++)
                {
                    var vendor = sampleVendors[row];
                    worksheet.Cells[row + 2, 1].Value = vendor.Name;
                    worksheet.Cells[row + 2, 2].Value = vendor.ContactPerson;
                    worksheet.Cells[row + 2, 3].Value = vendor.Email;
                    worksheet.Cells[row + 2, 4].Value = vendor.Phone;
                    worksheet.Cells[row + 2, 5].Value = vendor.Address;
                    worksheet.Cells[row + 2, 6].Value = vendor.TaxNumber;
                    worksheet.Cells[row + 2, 7].Value = vendor.PaymentTerms;
                }

                // Add instructions
                worksheet.Cells[6, 1].Value = "INSTRUCTIONS:";
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[7, 1].Value = "1. Only 'Name' column is required";
                worksheet.Cells[8, 1].Value = "2. Keep the header row as is";
                worksheet.Cells[9, 1].Value = "3. Add your vendors below the sample data";
                worksheet.Cells[10, 1].Value = "4. Delete sample rows if not needed";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Vendor_Import_Template_With_Samples_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating vendor template");
                TempData["ErrorMessage"] = "Error generating template file";
                return RedirectToAction(nameof(ImportVendors));
            }
        }

        #region Private Method
        private async Task<ImportResultViewModel> ProcessExcelFile(IFormFile excelFile, bool updateExisting, bool skipErrors)
        {
            var result = new ImportResultViewModel();
            var importedVendors = new List<Vendor>();
            var errors = new List<ImportError>();

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0]; // First worksheet

            if (worksheet == null)
            {
                throw new Exception("No worksheets found in the Excel file");
            }

            int rowCount = worksheet.Dimension.Rows;
            result.TotalRows = rowCount - 1; // Exclude header row

            // Validate header row
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header.ToLower()] = col;
                }
            }

            // Required headers check
            var requiredHeaders = new[] { "name" };
            foreach (var requiredHeader in requiredHeaders)
            {
                if (!headers.ContainsKey(requiredHeader))
                {
                    throw new Exception($"Required column '{requiredHeader}' not found in the Excel file");
                }
            }

            // Process data rows
            for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip header)
            {
                try
                {
                    var vendorRow = new ExcelVendorRow { RowNumber = row };

                    // Read cell values
                    vendorRow.Name = worksheet.Cells[row, headers["name"]].Value?.ToString()?.Trim() ?? string.Empty;

                    if (headers.ContainsKey("contactperson"))
                        vendorRow.ContactPerson = worksheet.Cells[row, headers["contactperson"]].Value?.ToString()?.Trim();

                    if (headers.ContainsKey("email"))
                        vendorRow.Email = worksheet.Cells[row, headers["email"]].Value?.ToString()?.Trim();

                    if (headers.ContainsKey("phone"))
                        vendorRow.Phone = worksheet.Cells[row, headers["phone"]].Value?.ToString()?.Trim();

                    if (headers.ContainsKey("address"))
                        vendorRow.Address = worksheet.Cells[row, headers["address"]].Value?.ToString()?.Trim();

                    if (headers.ContainsKey("taxnumber"))
                        vendorRow.TaxNumber = worksheet.Cells[row, headers["taxnumber"]].Value?.ToString()?.Trim();

                    if (headers.ContainsKey("paymentterms"))
                        vendorRow.PaymentTerms = worksheet.Cells[row, headers["paymentterms"]].Value?.ToString()?.Trim();

                    // Validate the row
                    var validationContext = new ValidationContext(vendorRow);
                    var validationResults = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(vendorRow, validationContext, validationResults, true);

                    if (!isValid)
                    {
                        var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();

                        if (skipErrors)
                        {
                            errors.Add(new ImportError
                            {
                                RowNumber = row,
                                VendorName = vendorRow.Name ?? "Unknown",
                                ErrorMessage = string.Join("; ", errorMessages)
                            });
                            result.ErrorCount++;
                            continue;
                        }
                        else
                        {
                            throw new Exception($"Row {row}: {string.Join("; ", errorMessages)}");
                        }
                    }

                    // Check if vendor already exists
                    var existingVendor = await _context.Vendors
                        .FirstOrDefaultAsync(v => v.Name == vendorRow.Name);

                    if (existingVendor != null)
                    {
                        if (updateExisting)
                        {
                            // Update existing vendor
                            existingVendor.ContactPerson = vendorRow.ContactPerson;
                            existingVendor.Email = vendorRow.Email;
                            existingVendor.Phone = vendorRow.Phone;
                            existingVendor.Address = vendorRow.Address;
                            existingVendor.TaxNumber = vendorRow.TaxNumber;
                            existingVendor.PaymentTerms = vendorRow.PaymentTerms;
                            existingVendor.IsActive = true;

                            result.UpdatedCount++;
                            importedVendors.Add(existingVendor);
                        }
                        else
                        {
                            errors.Add(new ImportError
                            {
                                RowNumber = row,
                                VendorName = vendorRow.Name,
                                ErrorMessage = "Vendor already exists and update is disabled"
                            });
                            result.ErrorCount++;
                            continue;
                        }
                    }
                    else
                    {
                        // Create new vendor
                        var newVendor = new Vendor
                        {
                            Name = vendorRow.Name,
                            ContactPerson = vendorRow.ContactPerson,
                            Email = vendorRow.Email,
                            Phone = vendorRow.Phone,
                            Address = vendorRow.Address,
                            TaxNumber = vendorRow.TaxNumber,
                            PaymentTerms = vendorRow.PaymentTerms,
                            CreatedDate = DateTime.Now,
                            IsActive = true
                        };

                        _context.Vendors.Add(newVendor);
                        importedVendors.Add(newVendor);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = row,
                        VendorName = "Unknown",
                        ErrorMessage = ex.Message
                    });
                    result.ErrorCount++;

                    if (!skipErrors)
                    {
                        throw;
                    }
                }
            }

            // Save all changes to database
            if (result.SuccessCount > 0 || result.UpdatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            result.Errors = errors;
            result.ImportedVendors = importedVendors;

            return result;
        }
        #endregion

        #region Blank Template
        // GET: /Inventory/DownloadBlankTemplate (Minimal Version)
        public IActionResult DownloadBlankTemplate()
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Vendors");

                // Just the headers - nothing else
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "ContactPerson";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Phone";
                worksheet.Cells[1, 5].Value = "Address";
                worksheet.Cells[1, 6].Value = "TaxNumber";
                worksheet.Cells[1, 7].Value = "PaymentTerms";

                // Make headers bold
                worksheet.Cells[1, 1, 1, 7].Style.Font.Bold = true;

                // Auto-fit
                worksheet.Cells[1, 1, 1, 7].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Vendor_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blank vendor template");
                TempData["ErrorMessage"] = "Error generating template file";
                return RedirectToAction(nameof(ImportVendors));
            }
        }
        #endregion
        #endregion
    }
}
