namespace Refactored.Domain.V1
{
    public class ProductOption : IProductOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IProductOption Clone()
        {
            return (ProductOption)this.MemberwiseClone();
        }
    }
}
