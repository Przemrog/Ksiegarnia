using System.ComponentModel.DataAnnotations;

namespace Ksiegarnia.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Range(0.5, 5)]
        public double Rating { get; set; }
    }
}
