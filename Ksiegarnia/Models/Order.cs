namespace Ksiegarnia.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public double SumPrice {  get; set; } 
    }
}
