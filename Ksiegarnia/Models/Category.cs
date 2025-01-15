using System.ComponentModel.DataAnnotations.Schema;

namespace Ksiegarnia.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
