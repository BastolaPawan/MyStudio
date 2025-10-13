using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStudio.Models;
using MyStudio.Models.DatabaseContext;
using System.ComponentModel.DataAnnotations;

namespace MyStudio.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoansController> _logger;

        public LoansController(ApplicationDbContext context, ILogger<LoansController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Where(l => l.LoanStatus == "Active")
                .OrderBy(l => l.LoanAccountNumber)
                .ToListAsync();

            return View(loans);
        }

        // GET: /Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Installments)
                .Include(l => l.InterestRateHistory)
                .Include(l => l.Transactions) // FIXED: Now this will work
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // GET: /Loans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoanAccountNumber,LoanType,InitialLoanAmount,StartDate,LoanTenureYears,CurrentInterestRate")] Loan loan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if loan account number already exists
                    if (await _context.Loans.AnyAsync(l => l.LoanAccountNumber == loan.LoanAccountNumber))
                    {
                        ModelState.AddModelError("LoanAccountNumber", "Loan account number already exists");
                        return View(loan);
                    }

                    // Calculate derived values - FIXED: Use CurrentInterestRate and proper percentage calculation
                    var totalMonths = loan.LoanTenureYears * 12;
                    var monthlyRate = loan.CurrentInterestRate / 100 / 12; // FIXED: Divide by 100 to convert percentage to decimal, then by 12 for monthly
                    var emiAmount = CalculateEMI(loan.InitialLoanAmount, monthlyRate, totalMonths);
                    var endDate = loan.StartDate.AddYears(loan.LoanTenureYears);

                    // Set calculated properties
                    loan.InstallmentAmount = emiAmount;
                    loan.EndDate = endDate;
                    loan.FinalInstallmentDate = endDate;
                    loan.NextInstallmentDate = loan.StartDate;
                    loan.TotalInstallments = totalMonths;
                    loan.NoOfInstallmentsRemaining = totalMonths;
                    loan.OutstandingPrincipal = loan.InitialLoanAmount;
                    loan.LoanStatus = "Active";
                    loan.CreatedDate = DateTime.Now;
                    loan.ModifiedDate = DateTime.Now;

                    _context.Add(loan);
                    await _context.SaveChangesAsync();

                    // Add initial interest rate history - FIXED: Use CurrentInterestRate
                    var interestHistory = new InterestRateHistory
                    {
                        LoanId = loan.LoanId,
                        InterestRate = loan.CurrentInterestRate, // FIXED: Use CurrentInterestRate
                        EffectiveFrom = loan.StartDate,
                        ChangedByUser = User.Identity?.Name ?? "System",
                        ReasonForChange = "Initial loan creation",
                        CreatedDate = DateTime.Now
                    };

                    _context.InterestRateHistory.Add(interestHistory);
                    await _context.SaveChangesAsync();

                    // Generate amortization schedule
                    await GenerateAmortizationSchedule(loan.LoanId);

                    TempData["SuccessMessage"] = "Loan created successfully!";
                    return RedirectToAction(nameof(Details), new { id = loan.LoanId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating loan");
                    ModelState.AddModelError("", "An error occurred while creating the loan. Please try again.");
                }
            }
            return View(loan);
        }

        // GET: /Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }
            return View(loan);
        }

        // POST: /Loans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LoanId,LoanType,LoanStatus")] Loan loanUpdate)
        {
            if (id != loanUpdate.LoanId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLoan = await _context.Loans.FindAsync(id);
                    if (existingLoan == null)
                    {
                        return NotFound();
                    }

                    // Only update allowed fields
                    existingLoan.LoanType = loanUpdate.LoanType;
                    existingLoan.LoanStatus = loanUpdate.LoanStatus;
                    existingLoan.ModifiedDate = DateTime.Now;

                    _context.Update(existingLoan);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Loan updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = existingLoan.LoanId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loanUpdate.LoanId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating loan");
                    ModelState.AddModelError("", "An error occurred while updating the loan. Please try again.");
                }
            }
            return View(loanUpdate);
        }

        // GET: /Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .FirstOrDefaultAsync(m => m.LoanId == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // POST: /Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Installments)
                    .Include(l => l.Transactions)
                    .Include(l => l.InterestRateHistory)
                    .FirstOrDefaultAsync(l => l.LoanId == id);

                if (loan == null)
                {
                    return NotFound();
                }

                // Check if loan has any payments made
                bool hasPayments = loan.Transactions?.Any() == true ||
                                  loan.Installments?.Any(i => i.InstallmentStatus == "Paid") == true;

                if (hasPayments)
                {
                    TempData["ErrorMessage"] = "Cannot delete loan that has payments recorded. Please close the loan instead.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Remove all related records
                _context.InterestRateHistory.RemoveRange(loan.InterestRateHistory);
                _context.LoanTransactions.RemoveRange(loan.Transactions);
                _context.LoanInstallments.RemoveRange(loan.Installments);
                _context.Loans.Remove(loan);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Loan deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan {LoanId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the loan. Please try again.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // POST: /Loans/Close/5 (Soft delete - recommended approach)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(id);
                if (loan == null)
                {
                    return NotFound();
                }

                // Soft delete by changing status
                loan.LoanStatus = "Closed";
                loan.ModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Loan closed successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing loan {LoanId}", id);
                TempData["ErrorMessage"] = "An error occurred while closing the loan. Please try again.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // GET: /Loans/UpdateInterestRate/5
        public async Task<IActionResult> UpdateInterestRate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
            {
                return NotFound();
            }

            ViewBag.LoanAccountNumber = loan.LoanAccountNumber;
            ViewBag.CurrentInterestRate = loan.CurrentInterestRate;

            //AUTO-POPULATE: Get next due date for Effective From
            var nextDueDate = loan.NextInstallmentDate;
            ViewBag.DefaultEffectiveFrom = nextDueDate.ToString("yyyy-MM-dd");

            return View();
        }

        //// POST: /Loans/UpdateInterestRate/5 (SOLUTION 1 - Updated)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateInterestRate(int id, [Bind("NewInterestRate,EffectiveFrom,ReasonForChange")] InterestRateUpdateModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var loan = await _context.Loans
        //                .Include(l => l.InterestRateHistory)
        //                .Include(l => l.Installments)
        //                .FirstOrDefaultAsync(l => l.LoanId == id);

        //            if (loan == null)
        //            {
        //                return NotFound();
        //            }

        //            // Close previous interest rate period
        //            var currentRate = loan.InterestRateHistory
        //                .FirstOrDefault(ir => ir.EffectiveTill == null);

        //            if (currentRate != null)
        //            {
        //                currentRate.EffectiveTill = model.EffectiveFrom.AddDays(-1);
        //            }

        //            // Add new interest rate history
        //            var newRate = new InterestRateHistory
        //            {
        //                LoanId = loan.LoanId,
        //                InterestRate = model.NewInterestRate,
        //                EffectiveFrom = model.EffectiveFrom,
        //                ChangedByUser = User.Identity?.Name ?? "System",
        //                ReasonForChange = model.ReasonForChange,
        //                CreatedDate = DateTime.Now
        //            };

        //            _context.InterestRateHistory.Add(newRate);

        //            // Update current interest rate
        //            loan.CurrentInterestRate = model.NewInterestRate;
        //            loan.ModifiedDate = DateTime.Now;

        //            await _context.SaveChangesAsync();

        //            // SOLUTION 2: Recalculate remaining installments with new interest rate
        //            await RecalculateRemainingInstallments(loan.LoanId, model.EffectiveFrom);

        //            TempData["SuccessMessage"] = "Interest rate updated and payment schedule recalculated successfully!";
        //            return RedirectToAction(nameof(Details), new { id = loan.LoanId });
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error updating interest rate");
        //            ModelState.AddModelError("", "An error occurred while updating the interest rate. Please try again.");
        //        }
        //    }

        //    var loanForView = await _context.Loans.FindAsync(id);
        //    if (loanForView != null)
        //    {
        //        ViewBag.LoanAccountNumber = loanForView.LoanAccountNumber;
        //        ViewBag.CurrentInterestRate = loanForView.CurrentInterestRate;
        //    }
        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInterestRate(int id, [Bind("NewInterestRate,EffectiveFrom,ReasonForChange")] InterestRateUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loan = await _context.Loans
                        .Include(l => l.InterestRateHistory)
                        .Include(l => l.Installments) // ✅ ADD THIS
                        .FirstOrDefaultAsync(l => l.LoanId == id);

                    if (loan == null)
                    {
                        return NotFound();
                    }

                    // Your existing rate update logic...
                    var currentRate = loan.InterestRateHistory
                        .FirstOrDefault(ir => ir.EffectiveTill == null);

                    if (currentRate != null)
                    {
                        //currentRate.EffectiveTill = model.EffectiveFrom.AddDays(-1);
                        currentRate.EffectiveTill = model.EffectiveFrom;
                    }

                    // Add new interest rate history
                    var newRate = new InterestRateHistory
                    {
                        LoanId = loan.LoanId,
                        InterestRate = model.NewInterestRate,
                        EffectiveFrom = model.EffectiveFrom,
                        ChangedByUser = User.Identity?.Name ?? "System",
                        ReasonForChange = model.ReasonForChange,
                        CreatedDate = DateTime.Now
                    };

                    _context.InterestRateHistory.Add(newRate);

                    // Update current interest rate
                    loan.CurrentInterestRate = model.NewInterestRate;
                    loan.ModifiedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // NEW: Recalculate pending installments from EffectiveFrom date
                    await RecalculatePendingInstallments(loan.LoanId, model.EffectiveFrom, model.NewInterestRate);

                    TempData["SuccessMessage"] = "Interest rate updated and payment schedule recalculated successfully!";
                    return RedirectToAction(nameof(Details), new { id = loan.LoanId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating interest rate");
                    ModelState.AddModelError("", "An error occurred while updating the interest rate. Please try again.");
                }
            }

            // Reload view data...
            return View(model);
        }
        private async Task RecalculatePendingInstallments(int loanId, DateTime effectiveFrom, decimal newRate)
        {
            var loan = await _context.Loans
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null) return;

            var pendingInstallments = loan.Installments
                .Where(i => i.InstallmentStatus == "Pending" && i.DueDate >= effectiveFrom)
                .OrderBy(i => i.DueDate)
                .ToList();

            if (!pendingInstallments.Any()) return;

            var lastPaidInstallment = loan.Installments
                .Where(i => i.InstallmentStatus == "Paid")
                .OrderByDescending(i => i.DueDate)
                .FirstOrDefault();

            var currentBalance = lastPaidInstallment?.ClosingBalance ?? loan.InitialLoanAmount;

            // ✅ ADD DEBUG LOGGING HERE
            _logger.LogInformation($"🔍 RECALCULATION DEBUG:");
            _logger.LogInformation($"   - LoanId: {loanId}");
            _logger.LogInformation($"   - Current Balance: {currentBalance}");
            _logger.LogInformation($"   - New Rate: {newRate}%");
            _logger.LogInformation($"   - Remaining Months: {pendingInstallments.Count}");
            _logger.LogInformation($"   - Effective From: {effectiveFrom:yyyy-MM-dd}");

            var remainingMonths = pendingInstallments.Count;
            var monthlyRate = newRate / 100 / 12;

            _logger.LogInformation($"   - Monthly Rate: {monthlyRate}");

            var newEMI = CalculateEMI(currentBalance, monthlyRate, remainingMonths);

            _logger.LogInformation($"   - Calculated EMI: {newEMI}");
            // ✅ END DEBUG LOGGING

            // Update loan EMI amount
            loan.InstallmentAmount = newEMI;

            foreach (var installment in pendingInstallments)
            {
                var previousDueDate = installment.InstallmentNumber == 1
                    ? loan.StartDate
                    : loan.StartDate.AddMonths(installment.InstallmentNumber - 1);

                var daysInPeriod = (installment.DueDate - previousDueDate).Days;
                var dailyRate = newRate / 100 / 365;

                var interestComponent = currentBalance * dailyRate * daysInPeriod;
                var principalComponent = newEMI - interestComponent;
                var closingBalance = currentBalance - principalComponent;

                if (installment == pendingInstallments.Last())
                {
                    principalComponent = currentBalance;
                    closingBalance = 0;
                }

                installment.InstallmentAmount = Math.Round(newEMI, 2);
                installment.InterestComponent = Math.Round(interestComponent, 2);
                installment.PrincipalComponent = Math.Round(principalComponent, 2);
                installment.OpeningBalance = Math.Round(currentBalance, 2);
                installment.ClosingBalance = Math.Round(closingBalance, 2);
                installment.DaysInPeriod = daysInPeriod;

                currentBalance = closingBalance;
            }

            await _context.SaveChangesAsync();
        }

        private decimal AdjustEMIForActualDays(decimal openingBalance, List<LoanInstallment> installments, decimal annualRate, decimal initialEMI)
        {
            decimal currentBalance = openingBalance;
            decimal adjustedEMI = initialEMI;

            // Iterative adjustment to find EMI that gives zero final balance
            for (int i = 0; i < 10; i++) // Max 10 iterations for convergence
            {
                currentBalance = openingBalance;

                foreach (var installment in installments)
                {
                    var previousDueDate = installment.InstallmentNumber == 1
                        ? installment.DueDate.AddMonths(-1) // This should be loan.StartDate in real scenario
                        : installment.DueDate.AddMonths(-1);

                    var daysInPeriod = (installment.DueDate - previousDueDate).Days;
                    var dailyRate = annualRate / 100 / 365;

                    var interest = currentBalance * dailyRate * daysInPeriod;
                    var principal = adjustedEMI - interest;
                    currentBalance -= principal;
                }

                // If we're close to zero, stop
                if (Math.Abs(currentBalance) < 1.0m)
                    break;

                // Adjust EMI based on remaining balance
                var remainingMonths = installments.Count;
                var monthlyRate = annualRate / 100 / 12;
                var adjustment = CalculateEMI(Math.Abs(currentBalance), monthlyRate, remainingMonths);

                if (currentBalance > 0)
                    adjustedEMI += adjustment / remainingMonths;
                else
                    adjustedEMI -= adjustment / remainingMonths;
            }

            return Math.Round(adjustedEMI, 2);
        }
        // SOLUTION 2: Recalculate Remaining Installments Method
        private async Task RecalculateRemainingInstallments(int loanId, DateTime effectiveFrom)
        {
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Installments)
                    .Include(l => l.InterestRateHistory)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId);

                if (loan == null) return;

                // Find the last paid installment to get current outstanding principal
                var lastPaidInstallment = loan.Installments
                    .Where(i => i.InstallmentStatus == "Paid")
                    .OrderByDescending(i => i.InstallmentNumber)
                    .FirstOrDefault();

                // Get installments that are pending and due after the effective date
                var remainingInstallments = loan.Installments
                    .Where(i => i.InstallmentStatus == "Pending" && i.DueDate >= effectiveFrom)
                    .OrderBy(i => i.InstallmentNumber)
                    .ToList();

                if (!remainingInstallments.Any())
                {
                    _logger.LogInformation("No pending installments found to recalculate for loan {LoanId}", loanId);
                    return;
                }

                // Get current outstanding principal
                var outstandingPrincipal = lastPaidInstallment?.ClosingBalance ?? loan.InitialLoanAmount;
                var remainingMonths = remainingInstallments.Count;
                var monthlyRate = loan.CurrentInterestRate / 100 / 12;

                // Recalculate EMI with new interest rate for remaining tenure
                var newEMI = CalculateEMI(outstandingPrincipal, monthlyRate, remainingMonths);

                // Update loan EMI amount
                loan.InstallmentAmount = newEMI;
                loan.ModifiedDate = DateTime.Now;

                // Recalculate remaining installments with new interest rate
                var openingBalance = outstandingPrincipal;

                foreach (var installment in remainingInstallments)
                {
                    var interestComponent = openingBalance * monthlyRate;
                    var principalComponent = newEMI - interestComponent;
                    var closingBalance = openingBalance - principalComponent;

                    // For last installment, adjust to ensure zero balance
                    if (installment == remainingInstallments.Last())
                    {
                        principalComponent = openingBalance;
                        closingBalance = 0;
                        newEMI = principalComponent + interestComponent; // Adjust final EMI
                    }

                    // Update installment with new calculated values
                    installment.InstallmentAmount = Math.Round(newEMI, 2);
                    installment.PrincipalComponent = Math.Round(principalComponent, 2);
                    installment.InterestComponent = Math.Round(interestComponent, 2);
                    installment.OpeningBalance = Math.Round(openingBalance, 2);
                    installment.ClosingBalance = Math.Round(closingBalance, 2);

                    openingBalance = closingBalance;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Recalculated {Count} installments for loan {LoanId} with new interest rate {InterestRate}%",
                    remainingInstallments.Count, loanId, loan.CurrentInterestRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating installments for loan {LoanId}", loanId);
                throw; // Re-throw to handle in calling method
            }
        }

        #region If you want only to pay installment on instalment date or after installment date replace commented code with uncommented one.
        // GET: /Loans/MakePayment/5
        //public async Task<IActionResult> MakePayment(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var loan = await _context.Loans
        //        .Include(l => l.Installments)
        //        .FirstOrDefaultAsync(l => l.LoanId == id);

        //    if (loan == null)
        //    {
        //        return NotFound();
        //    }

        //    var dueInstallment = loan.Installments
        //        .FirstOrDefault(i => i.InstallmentStatus == "Pending" && i.DueDate <= DateTime.Now);

        //    if (dueInstallment == null)
        //    {
        //        TempData["ErrorMessage"] = "No due installments found for payment.";
        //        return RedirectToAction(nameof(Details), new { id = loan.LoanId });
        //    }

        //    var paymentModel = new PaymentModel
        //    {
        //        LoanId = loan.LoanId,
        //        LoanAccountNumber = loan.LoanAccountNumber,
        //        DueInstallmentNumber = dueInstallment.InstallmentNumber,
        //        DueDate = dueInstallment.DueDate,
        //        Amount = dueInstallment.InstallmentAmount,
        //        PaymentDate = DateTime.Now
        //    };

        //    return View(paymentModel);
        //}
        #endregion
        public async Task<IActionResult> MakePayment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
            {
                return NotFound();
            }

            // Find the NEXT pending installment (even if due in future)
            var nextInstallment = loan.Installments
                .Where(i => i.InstallmentStatus == "Pending")
                .OrderBy(i => i.DueDate)
                .FirstOrDefault();

            if (nextInstallment == null)
            {
                TempData["ErrorMessage"] = "No pending installments found. All installments may already be paid.";
                return RedirectToAction(nameof(Details), new { id = loan.LoanId });
            }

            var paymentModel = new PaymentModel
            {
                LoanId = loan.LoanId,
                LoanAccountNumber = loan.LoanAccountNumber,
                DueInstallmentNumber = nextInstallment.InstallmentNumber,
                DueDate = nextInstallment.DueDate,
                Amount = nextInstallment.InstallmentAmount,
                PaymentDate = nextInstallment.DueDate
            };

            return View(paymentModel);
        }
        // POST: /Loans/MakePayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(int id, PaymentModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loan = await _context.Loans
                        .Include(l => l.Installments)
                        .FirstOrDefaultAsync(l => l.LoanId == id);

                    if (loan == null)
                    {
                        return NotFound();
                    }

                    var dueInstallment = loan.Installments
                        .FirstOrDefault(i => i.InstallmentNumber == model.DueInstallmentNumber && i.InstallmentStatus == "Pending");

                    if (dueInstallment == null)
                    {
                        ModelState.AddModelError("", "Installment not found or already paid.");
                        return View(model);
                    }

                    // Create transaction
                    var transaction = new LoanTransaction
                    {
                        LoanId = loan.LoanId,
                        InstallmentId = dueInstallment.InstallmentId,
                        TransactionDate = model.PaymentDate,
                        TransactionType = "EMI",
                        Amount = model.Amount,
                        PrincipalAmount = dueInstallment.PrincipalComponent,
                        InterestAmount = dueInstallment.InterestComponent,
                        PaymentMethod = model.PaymentMethod,
                        ReferenceNumber = model.ReferenceNumber,
                        Remarks = model.Remarks,
                        CreatedDate = DateTime.Now
                    };

                    // Update installment
                    dueInstallment.PaidDate = model.PaymentDate;
                    dueInstallment.PaidAmount = model.Amount;
                    dueInstallment.PrincipalPaid = dueInstallment.PrincipalComponent;
                    dueInstallment.InterestPaid = dueInstallment.InterestComponent;
                    dueInstallment.InstallmentStatus = "Paid";

                    // Update loan summary
                    loan.InstallmentsPaidTillDate += 1;
                    loan.NoOfInstallmentsRemaining -= 1;
                    loan.OutstandingPrincipal = dueInstallment.ClosingBalance;
                    loan.LastInstallmentDate = model.PaymentDate;

                    var nextInstallment = loan.Installments
                        .FirstOrDefault(i => i.InstallmentStatus == "Pending");
                    loan.NextInstallmentDate = nextInstallment?.DueDate ?? loan.FinalInstallmentDate;

                    loan.ModifiedDate = DateTime.Now;

                    _context.LoanTransactions.Add(transaction);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Payment of {model.Amount:C} processed successfully!";
                    return RedirectToAction(nameof(Details), new { id = loan.LoanId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment");
                    ModelState.AddModelError("", "An error occurred while processing the payment. Please try again.");
                }
            }
            return View(model);
        }

        // POST: /Loans/DeletePayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int transactionId)
        {
            int? loanId = null;
            try
            {
                var transaction = await _context.LoanTransactions
                    .Include(t => t.Installment)
                    .Include(t => t.Loan)
                    .ThenInclude(l => l.Installments)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                {
                    return NotFound();
                }

                loanId = transaction.LoanId;
                var installmentNumber = transaction.Installment?.InstallmentNumber;

                using var transactionScope = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Reset the installment status and payment details
                    if (transaction.Installment != null)
                    {
                        transaction.Installment.PaidDate = null;
                        transaction.Installment.PaidAmount = 0;
                        transaction.Installment.PrincipalPaid = 0;
                        transaction.Installment.InterestPaid = 0;
                        transaction.Installment.LateFee = 0;
                        transaction.Installment.InstallmentStatus = "Pending";
                    }

                    // 2. Remove the transaction
                    _context.LoanTransactions.Remove(transaction);

                    // 3. Update loan summary
                    var loan = transaction.Loan;
                    if (loan != null)
                    {
                        // Recalculate installments paid
                        loan.InstallmentsPaidTillDate = await _context.LoanInstallments
                            .CountAsync(i => i.LoanId == loanId && i.InstallmentStatus == "Paid");

                        loan.NoOfInstallmentsRemaining = loan.TotalInstallments - loan.InstallmentsPaidTillDate;

                        // Recalculate outstanding principal
                        var lastPaidInstallment = await _context.LoanInstallments
                            .Where(i => i.LoanId == loanId && i.InstallmentStatus == "Paid")
                            .OrderByDescending(i => i.InstallmentNumber)
                            .FirstOrDefaultAsync();

                        loan.OutstandingPrincipal = lastPaidInstallment?.ClosingBalance ?? loan.InitialLoanAmount;

                        // Update next installment date
                        var nextInstallment = await _context.LoanInstallments
                            .Where(i => i.LoanId == loanId && i.InstallmentStatus == "Pending")
                            .OrderBy(i => i.DueDate)
                            .FirstOrDefaultAsync();

                        loan.NextInstallmentDate = nextInstallment?.DueDate ?? loan.FinalInstallmentDate;
                        loan.ModifiedDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    await transactionScope.CommitAsync();

                    TempData["SuccessMessage"] = $"Payment transaction deleted successfully. Installment #{installmentNumber} has been reset to pending.";
                }
                catch (Exception)
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                return RedirectToAction(nameof(Details), new { id = loanId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment transaction {TransactionId}", transactionId);
                TempData["ErrorMessage"] = "An error occurred while deleting the payment. Please try again.";
                return RedirectToAction(nameof(Details), new { id = loanId });
            }
        }

        // GET: /Loans/ConfirmDeletePayment/5 (Enhanced)
        public async Task<IActionResult> ConfirmDeletePayment(int? transactionId)
        {
            if (transactionId == null)
            {
                return NotFound();
            }

            var transaction = await _context.LoanTransactions
                .Include(t => t.Loan)
                .Include(t => t.Installment)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return NotFound();
            }

            // Check if payment can be deleted
            var canDelete = await CanDeletePayment(transactionId.Value);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "This payment cannot be deleted because it's not the most recent payment. Only the most recent payment can be deleted to maintain data integrity.";
                return RedirectToAction(nameof(Details), new { id = transaction.LoanId }); // Fixed: use transaction.LoanId
            }

            ViewBag.CanDelete = canDelete;
            return View(transaction);
        }

        // Enhanced version with additional safety checks
        private async Task<bool> CanDeletePayment(int transactionId)
        {
            var transaction = await _context.LoanTransactions
                .Include(t => t.Installment)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) return false;

            // Check if this is the most recent payment
            var latestTransaction = await _context.LoanTransactions
                .Where(t => t.LoanId == transaction.LoanId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.TransactionId)
                .FirstOrDefaultAsync();

            // Only allow deletion of the most recent payment to maintain data integrity
            if (latestTransaction?.TransactionId != transactionId)
            {
                return false;
            }

            return true;
        }

        // Helper method to get effective interest rate for a specific date
        private decimal GetEffectiveInterestRate(Loan loan, DateTime forDate)
        {
            var effectiveRate = loan.InterestRateHistory
                .Where(ir => ir.EffectiveFrom <= forDate &&
                            (ir.EffectiveTill == null || ir.EffectiveTill >= forDate))
                .OrderByDescending(ir => ir.EffectiveFrom)
                .FirstOrDefault();

            return effectiveRate?.InterestRate ?? loan.CurrentInterestRate;
        }
        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.LoanId == id);
        }

        //private decimal CalculateEMI(decimal principal, decimal monthlyRate, int tenureMonths)
        //{
        //    var emi = principal * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, tenureMonths) /
        //             (decimal)(Math.Pow(1 + (double)monthlyRate, tenureMonths) - 1);

        //    return Math.Round(emi, 2);
        //}
        // Enhanced EMI Calculation Method
        private decimal CalculateEMI(decimal principal, decimal monthlyRate, int tenureMonths)
        {
            if (monthlyRate == 0)
            {
                // If interest rate is 0%, EMI is simply principal divided by months
                return Math.Round(principal / tenureMonths, 2);
            }

            var emi = principal * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, tenureMonths) /
                     (decimal)(Math.Pow(1 + (double)monthlyRate, tenureMonths) - 1);

            return Math.Round(emi, 2);
        }
        private async Task GenerateAmortizationSchedule(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return;

            var monthlyRate = loan.CurrentInterestRate / 100 / 12;
            var openingBalance = loan.InitialLoanAmount;
            var emi = loan.InstallmentAmount;

            // Clear any existing installments
            var existingInstallments = _context.LoanInstallments.Where(li => li.LoanId == loanId);
            _context.LoanInstallments.RemoveRange(existingInstallments);

            for (int i = 1; i <= loan.TotalInstallments; i++)
            {
                DateTime periodStartDate, dueDate;
                int daysInPeriod;

                if (i == 1)
                {
                    // First installment: from start date to first due date
                    periodStartDate = loan.StartDate;
                    dueDate = loan.StartDate;
                    daysInPeriod = (dueDate - periodStartDate).Days;
                }
                else
                {
                    // Subsequent installments: from previous due date to current due date
                    periodStartDate = loan.StartDate.AddMonths(i - 2);
                    dueDate = loan.StartDate.AddMonths(i - 1);
                    daysInPeriod = (dueDate - periodStartDate).Days;
                }

                // Calculate interest based on actual days
                var dailyRate = loan.CurrentInterestRate / 100 / 365;
                var interestComponent = openingBalance * dailyRate * daysInPeriod;

                var principalComponent = emi - interestComponent;
                var closingBalance = openingBalance - principalComponent;

                // For last installment, adjust to ensure zero balance
                if (i == loan.TotalInstallments)
                {
                    principalComponent = openingBalance;
                    closingBalance = 0;
                }

                var installment = new LoanInstallment
                {
                    LoanId = loan.LoanId,
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    InstallmentAmount = Math.Round(emi, 2),
                    PrincipalComponent = Math.Round(principalComponent, 2),
                    InterestComponent = Math.Round(interestComponent, 2),
                    OpeningBalance = Math.Round(openingBalance, 2),
                    ClosingBalance = Math.Round(closingBalance, 2),
                    InstallmentStatus = "Pending",
                    DaysInPeriod = daysInPeriod // Store the actual days
                };

                _context.LoanInstallments.Add(installment);
                openingBalance = closingBalance;
            }

            await _context.SaveChangesAsync();
        }
    }

    // View Models for MVC
    public class InterestRateUpdateModel
    {
        [Required]
        [Display(Name = "New Interest Rate")]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01% and 100%")]
        public decimal NewInterestRate { get; set; }

        [Required]
        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [Required]
        [Display(Name = "Reason for Change")]
        public string ReasonForChange { get; set; } = string.Empty;
    }

    public class PaymentModel
    {
        public int LoanId { get; set; }
        public string LoanAccountNumber { get; set; } = string.Empty;

        [Display(Name = "Installment Number")]
        public int DueInstallmentNumber { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        public string? Remarks { get; set; }
    }
}