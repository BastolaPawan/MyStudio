using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
