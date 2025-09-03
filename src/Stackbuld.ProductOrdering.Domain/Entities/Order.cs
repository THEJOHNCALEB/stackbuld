using System.ComponentModel.DataAnnotations;

namespace Stackbuld.ProductOrdering.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
    public decimal TotalAmount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public void Complete()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot complete order with status: {Status}");
        
        Status = OrderStatus.Completed;
    }

    public void Fail()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot fail order with status: {Status}");
        
        Status = OrderStatus.Failed;
    }

    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.LineTotal);
    }
}

public enum OrderStatus
{
    Pending,
    Completed,
    Failed
}
