using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    // ViewModels/InventoryViewModels.cs
    public class CreateItemViewModel
    {
        [Required]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Item Group")]
        public int ItemGroupId { get; set; }

        [Display(Name = "Sub Group")]
        public int? ItemSubGroupId { get; set; }

        [Display(Name = "Minimum Stock")]
        public decimal MinimumStock { get; set; }

        [Display(Name = "Maximum Stock")]
        public decimal MaximumStock { get; set; }

        [Display(Name = "Reorder Level")]
        public decimal ReorderLevel { get; set; }

        // Dropdown options
        public List<Unit> Units { get; set; } = new List<Unit>();
        public List<ItemGroup> ItemGroups { get; set; } = new List<ItemGroup>();
        public List<ItemSubGroup> ItemSubGroups { get; set; } = new List<ItemSubGroup>();
    }
 
}
