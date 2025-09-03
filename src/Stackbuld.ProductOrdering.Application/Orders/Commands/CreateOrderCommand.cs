using MediatR;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Interfaces;
using Stackbuld.ProductOrdering.Domain.Entities;
using Stackbuld.ProductOrdering.Domain.Exceptions;

namespace Stackbuld.ProductOrdering.Application.Orders.Commands;

public record CreateOrderCommand(CreateOrderDto Order) : IRequest<OrderDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate all products exist and have sufficient stock
            var productValidations = new List<(Guid ProductId, string ProductName, int AvailableStock, int RequestedQuantity)>();
            
            foreach (var item in request.Order.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                
                if (product == null)
                    throw new ArgumentException($"Product with ID {item.ProductId} not found");

                if (!product.HasStock(item.Quantity))
                {
                    productValidations.Add((product.Id, product.Name, product.StockQuantity, item.Quantity));
                }
            }

            // If any product has insufficient stock, fail the order
            if (productValidations.Any())
            {
                var firstValidation = productValidations.First();
                throw new InsufficientStockException(
                    firstValidation.ProductId,
                    firstValidation.ProductName,
                    firstValidation.AvailableStock,
                    firstValidation.RequestedQuantity);
            }

            // Create the order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Create order items and reduce stock atomically
            foreach (var item in request.Order.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                
                // Use atomic stock reservation
                var stockReserved = await _unitOfWork.Products.TryReserveStockAsync(
                    item.ProductId, 
                    item.Quantity, 
                    cancellationToken);

                if (!stockReserved)
                {
                    throw new InsufficientStockException(
                        product!.Id,
                        product.Name,
                        product.StockQuantity,
                        item.Quantity);
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    UnitPrice = product!.Price,
                    Quantity = item.Quantity
                };
                
                orderItem.CalculateLineTotal();
                order.Items.Add(orderItem);
            }

            order.CalculateTotal();
            order.Complete();

            var createdOrder = await _unitOfWork.Orders.CreateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new OrderDto
            {
                Id = createdOrder.Id,
                TotalAmount = createdOrder.TotalAmount,
                CreatedAt = createdOrder.CreatedAt,
                Status = createdOrder.Status.ToString(),
                Items = createdOrder.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
