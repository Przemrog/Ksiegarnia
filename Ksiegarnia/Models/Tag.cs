using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ksiegarnia.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<BookTag> BookTags { get; set; }
    }
}