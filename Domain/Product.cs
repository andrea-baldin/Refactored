namespace Refactored.Domain.V1
{
    public class Product : IProduct
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal DeliveryPrice { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; }

        public IProduct Clone()
        {
            return (Product)this.MemberwiseClone();
        }
    }
}