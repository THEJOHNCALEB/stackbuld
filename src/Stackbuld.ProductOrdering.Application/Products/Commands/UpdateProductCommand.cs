using MediatR;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Interfaces;

namespace Stackbuld.ProductOrdering.Application.Products.Commands;

public record UpdateProductCommand(Guid Id, UpdateProductDto Product) : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
            throw new ArgumentException($"Product with ID {request.Id} not found");

        product.Name = request.Product.Name;
        product.Description = request.Product.Description;
        product.Price = request.Product.Price;
        product.StockQuantity = request.Product.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto
        {
            Id = updatedProduct.Id,
            Name = updatedProduct.Name,
            Description = updatedProduct.Description,
            Price = updatedProduct.Price,
            StockQuantity = updatedProduct.StockQuantity,
            CreatedAt = updatedProduct.CreatedAt,
            UpdatedAt = updatedProduct.UpdatedAt
        };
    }
}
