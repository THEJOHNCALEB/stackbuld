using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Interfaces;
using Stackbuld.ProductOrdering.Application.Products.Commands;
using Stackbuld.ProductOrdering.Application.Products.Queries;
using Stackbuld.ProductOrdering.Domain.Entities;
using Stackbuld.ProductOrdering.Infrastructure.Data;
using Stackbuld.ProductOrdering.Infrastructure.Repositories;
using Xunit;

namespace Stackbuld.ProductOrdering.Tests;

public class ProductTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public ProductTests()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<UnitOfWork>();
        
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateProduct_ShouldSucceed()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            StockQuantity = 10
        };

        var command = new CreateProductCommand(createProductDto);
        var handler = new CreateProductCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createProductDto.Name, result.Name);
        Assert.Equal(createProductDto.Description, result.Description);
        Assert.Equal(createProductDto.Price, result.Price);
        Assert.Equal(createProductDto.StockQuantity, result.StockQuantity);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var query = new GetProductByIdQuery(product.Id);
        var handler = new GetProductByIdQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.00m, StockQuantity = 5, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.00m, StockQuantity = 3, CreatedAt = DateTime.UtcNow }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        var query = new GetAllProductsQuery();
        var handler = new GetAllProductsQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
