using System.ComponentModel.DataAnnotations;

namespace Stackbuld.ProductOrdering.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal UnitPrice { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Line total must be greater than 0")]
    public decimal LineTotal { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public void CalculateLineTotal()
    {
        LineTotal = UnitPrice * Quantity;
    }
}
