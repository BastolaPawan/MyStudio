using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    public class ItemSubGroup
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Foreign Key
        public int ItemGroupId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ItemGroup ItemGroup { get; set; } = null!;
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
