using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    // ViewModels/ImportVendorsViewModel.cs
    public class ImportVendorsViewModel
    {
        [Required]
        [Display(Name = "Excel File")]
        public IFormFile ExcelFile { get; set; }

        [Display(Name = "Update existing vendors")]
        public bool UpdateExisting { get; set; } = false;

        [Display(Name = "Skip rows with errors")]
        public bool SkipErrors { get; set; } = true;
    }

    // ViewModels/ImportResultViewModel.cs
    public class ImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public List<Vendor> ImportedVendors { get; set; } = new List<Vendor>();
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // ViewModels/ExcelVendorRow.cs
    public class ExcelVendorRow
    {
        public int RowNumber { get; set; }

        [Required(ErrorMessage = "Vendor Name is required")]
        public string Name { get; set; } = string.Empty;

        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? TaxNumber { get; set; }

        public string? PaymentTerms { get; set; }

        public bool IsValid { get; set; } = true;
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
