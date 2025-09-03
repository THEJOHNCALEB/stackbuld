namespace Stackbuld.ProductOrdering.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int AvailableStock { get; }
    public int RequestedQuantity { get; }

    public InsufficientStockException(Guid productId, string productName, int availableStock, int requestedQuantity)
        : base($"Insufficient stock for product '{productName}'. Available: {availableStock}, Requested: {requestedQuantity}")
    {
        ProductId = productId;
        ProductName = productName;
        AvailableStock = availableStock;
        RequestedQuantity = requestedQuantity;
    }
}
