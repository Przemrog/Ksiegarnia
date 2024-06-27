namespace Ksiegarnia.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart cart { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int Count { get; set; }
    }
}
