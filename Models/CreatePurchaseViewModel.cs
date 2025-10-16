using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    public class CreatePurchaseViewModel
    {
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Shipping Cost")]
        public decimal ShippingCost { get; set; }

        [Display(Name = "Tax Percentage")]
        public decimal TaxPercent { get; set; } = 13;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public List<PurchaseItemViewModel> Items { get; set; } = new List<PurchaseItemViewModel>();
        public List<Vendor> Vendors { get; set; } = new List<Vendor>();
        public List<Item> AvailableItems { get; set; } = new List<Item>();
    }
}
