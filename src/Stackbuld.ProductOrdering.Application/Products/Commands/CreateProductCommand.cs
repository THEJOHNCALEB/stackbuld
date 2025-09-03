using MediatR;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Interfaces;
using Stackbuld.ProductOrdering.Domain.Entities;

namespace Stackbuld.ProductOrdering.Application.Products.Commands;

public record CreateProductCommand(CreateProductDto Product) : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Product.Name,
            Description = request.Product.Description,
            Price = request.Product.Price,
            StockQuantity = request.Product.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _unitOfWork.Products.CreateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto
        {
            Id = createdProduct.Id,
            Name = createdProduct.Name,
            Description = createdProduct.Description,
            Price = createdProduct.Price,
            StockQuantity = createdProduct.StockQuantity,
            CreatedAt = createdProduct.CreatedAt,
            UpdatedAt = createdProduct.UpdatedAt
        };
    }
}
