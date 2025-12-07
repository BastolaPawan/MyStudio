using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;

namespace MyStudio.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillingController> _logger;

        public BillingController(ApplicationDbContext context, ILogger<BillingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Billing
        public IActionResult Index()
        {
            var model = new OrderSearchViewModel();
            return View(model);
        }

        // POST: /Billing/Search
        [HttpPost]
        public IActionResult Search(OrderSearchViewModel model)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Payments)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(model.OrderNumber))
                query = query.Where(o => o.OrderNumber.Contains(model.OrderNumber));

            if (!string.IsNullOrEmpty(model.CustomerName))
                query = query.Where(o => o.Customer.Name.Contains(model.CustomerName));

            if (!string.IsNullOrEmpty(model.Status))
                query = query.Where(o => o.Status == model.Status);

            if (!string.IsNullOrEmpty(model.PaymentStatus))
                query = query.Where(o => o.PaymentStatus == model.PaymentStatus);

            if (model.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= model.FromDate.Value);

            if (model.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= model.ToDate.Value);

            if (model.ShowUnpaidOnly)
                query = query.Where(o => o.BalanceAmount > 0 && !o.IsCancelled);

            if (model.ShowCancelled)
                query = query.Where(o => o.IsCancelled);
            else
                query = query.Where(o => !o.IsCancelled);

            if (model.ShowBadDebts)
                query = query.Where(o => o.IsBadDebt);

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            return View("Index", new OrderSearchViewModel { Results = orders });
        }

        // GET: /Billing/Create
        public IActionResult Create()
        {
            var model = new CreateOrderViewModel
            {
                Customers = _context.Customers.Where(c => c.IsActive).ToList(),
                ProductsServices = _context.ProductServices.Where(p => p.IsActive).ToList()
            };
            return View(model);
        }

        // POST: /Billing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (ModelState.IsValid && model.Items.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Generate order number
                    var orderCount = _context.Orders.Count(o => o.OrderDate.Year == DateTime.Now.Year);
                    var orderNumber = $"ORD-{DateTime.Now:yyyy}-{(orderCount + 1):D3}";

                    var order = new Order
                    {
                        OrderNumber = orderNumber,
                        CustomerId = model.CustomerId,
                        OrderType = model.OrderType,
                        SessionType = model.SessionType,
                        OrderDate = DateTime.Now,
                        DeliveryDate = model.DeliveryDate,
                        PhotoShootDate = model.PhotoShootDate,
                        DeliveryMethod = model.DeliveryMethod,
                        CourierName = model.CourierName,
                        DeliveryAddress = model.DeliveryAddress,
                        Status = "Confirmed",
                        Notes = model.Notes,
                        CreatedDate = DateTime.Now
                    };

                    // Add order items
                    foreach (var itemModel in model.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            ItemType = itemModel.ItemType,
                            Description = itemModel.Description,
                            Quantity = itemModel.Quantity,
                            UnitPrice = itemModel.UnitPrice,
                            DiscountPercent = itemModel.DiscountPercent,
                            LineTotal = itemModel.Quantity * itemModel.UnitPrice * (1 - itemModel.DiscountPercent / 100)
                        };
                        order.OrderItems.Add(orderItem);
                        order.SubTotal += orderItem.LineTotal;
                    }

                    // Apply customer discount for wholesale customers
                    var customer = await _context.Customers.FindAsync(model.CustomerId);
                    if (customer?.CustomerType == "Wholesale")
                    {
                        order.DiscountAmount = order.SubTotal * (customer.DiscountPercentage / 100);
                    }

                    // Calculate totals
                    order.TaxAmount = (order.SubTotal - order.DiscountAmount) * 0.13m; // 13% tax
                    order.ShippingCharge = order.DeliveryMethod == "Courier" ? 100 : 0; // Default courier charge
                    order.TotalAmount = order.SubTotal - order.DiscountAmount + order.TaxAmount + order.ShippingCharge;

                    // Process advance payment
                    if (model.AdvanceAmount > 0)
                    {
                        order.AdvancePaid = model.AdvanceAmount;
                        order.PaymentStatus = order.AdvancePaid >= order.TotalAmount ? "Paid" : "Partial";

                        var payment = new Payment
                        {
                            PaymentNumber = $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                            PaymentDate = DateTime.Now,
                            Amount = model.AdvanceAmount,
                            PaymentMethod = model.AdvancePaymentMethod,
                            PaymentType = "Advance",
                            Notes = "Advance payment at order creation"
                        };
                        order.Payments.Add(payment);
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Order {order.OrderNumber} created successfully!";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating order");
                    ModelState.AddModelError("", "Error creating order. Please try again.");
                }
            }

            // Reload dropdown data
            model.Customers = _context.Customers.Where(c => c.IsActive).ToList();
            model.ProductsServices = _context.ProductServices.Where(p => p.IsActive).ToList();
            return View(model);
        }

        // GET: /Billing/Details/5
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Billing/AddPayment/5
        public IActionResult AddPayment(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var model = new PaymentViewModel
            {
                OrderId = id,
                Amount = order.BalanceAmount
            };

            ViewBag.Order = order;
            return View(model);
        }

        // POST: /Billing/AddPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(PaymentViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);

            if (order == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var payment = new Payment
                    {
                        OrderId = model.OrderId,
                        PaymentNumber = $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                        PaymentDate = model.PaymentDate,
                        Amount = model.Amount,
                        PaymentMethod = model.PaymentMethod,
                        PaymentType = "Partial",
                        ReferenceNumber = model.ReferenceNumber,
                        Notes = model.Notes,
                        CreatedDate = DateTime.Now
                    };

                    _context.Payments.Add(payment);

                    // Update order payment status
                    var totalPaid = order.AdvancePaid + order.Payments.Sum(p => p.Amount) + model.Amount;
                    order.PaymentStatus = totalPaid >= order.TotalAmount ? "Paid" : "Partial";

                    // If fully paid and ready, mark as delivered
                    if (order.PaymentStatus == "Paid" && order.Status == "Ready")
                    {
                        order.Status = "Delivered";
                        order.ActualDeliveryDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Payment of {model.Amount:C} recorded successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.OrderId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error recording payment");
                    ModelState.AddModelError("", "Error recording payment. Please try again.");
                }
            }

            ViewBag.Order = order;
            return View(model);
        }

        // POST: /Billing/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string reason, decimal cancellationCharge = 0)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            try
            {
                order.IsCancelled = true;
                order.CancelledDate = DateTime.Now;
                order.CancellationReason = reason;
                order.CancellationCharge = cancellationCharge;
                order.Status = "Cancelled";

                // Mark as bad debt if advance paid but order cancelled
                if (order.AdvancePaid > 0 && cancellationCharge < order.AdvancePaid)
                {
                    order.IsBadDebt = true;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Order {order.OrderNumber} cancelled successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                TempData["ErrorMessage"] = "Error cancelling order. Please try again.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // POST: /Billing/MarkAsReady/5
        [HttpPost]
        public async Task<IActionResult> MarkAsReady(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            order.Status = "Ready";
            order.ReadyDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order marked as ready for delivery" });
        }

        // POST: /Billing/MarkAsDelivered/5
        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            order.Status = "Delivered";
            order.ActualDeliveryDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order marked as delivered" });
        }

        // GET: /Billing/UnpaidBills
        public IActionResult UnpaidBills()
        {
            var unpaidOrders = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Payments)
            .Where(o =>
                (o.TotalAmount - o.AdvancePaid - o.Payments.Sum(p => p.Amount)) > 0
                && !o.IsCancelled)
            .OrderBy(o => o.OrderDate)
            .ToList();

            return View(unpaidOrders);
        }

        // GET: /Billing/BadDebts
        public IActionResult BadDebts()
        {
            var badDebts = _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.IsBadDebt)
                .OrderByDescending(o => o.CancelledDate)
                .ToList();

            return View(badDebts);
        }
    }
}
