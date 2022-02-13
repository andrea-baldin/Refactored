
namespace Refactored.Domain.V1
{
    public interface IProduct
    {
        decimal DeliveryPrice { get; set; }
        string Description { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
        decimal Price { get; set; }

        IProduct Clone();
    }
}