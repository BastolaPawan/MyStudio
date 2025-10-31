using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    // ViewModels/CreateVendorViewModel.cs
    public class CreateVendorViewModel
    {
        [Required]
        [Display(Name = "Vendor Name")]
        [StringLength(100, ErrorMessage = "Vendor name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Tax Number")]
        public string? TaxNumber { get; set; }

        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; }
    }
}
