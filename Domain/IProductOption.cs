
namespace Refactored.Domain.V1
{
    public interface IProductOption
    {
        string Description { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
        Guid ProductId { get; set; }

        IProductOption Clone();
    }
}