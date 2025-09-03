using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Interfaces;
using Stackbuld.ProductOrdering.Application.Orders.Commands;
using Stackbuld.ProductOrdering.Application.Products.Commands;
using Stackbuld.ProductOrdering.Domain.Entities;
using Stackbuld.ProductOrdering.Infrastructure.Data;
using Stackbuld.ProductOrdering.Infrastructure.Repositories;
using Xunit;

namespace Stackbuld.ProductOrdering.Tests;

public class ConcurrencyTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public ConcurrencyTests()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<UnitOfWork>();
        
        // Create database
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ProductStockValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 5, // Only 5 items in stock
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.True(product.HasStock(3));
        Assert.True(product.HasStock(5));
        Assert.False(product.HasStock(6));
        Assert.False(product.HasStock(10));

        // Test stock reduction
        product.ReduceStock(2);
        Assert.Equal(3, product.StockQuantity);
        Assert.True(product.HasStock(3));
        Assert.False(product.HasStock(4));

        // Test insufficient stock exception
        Assert.Throws<InvalidOperationException>(() => product.ReduceStock(5));
    }

    [Fact]
    public async Task OrderCalculation_ShouldWorkCorrectly()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var orderItem1 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = Guid.NewGuid(),
            UnitPrice = 50.00m,
            Quantity = 2
        };
        orderItem1.CalculateLineTotal();

        var orderItem2 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = Guid.NewGuid(),
            UnitPrice = 25.00m,
            Quantity = 3
        };
        orderItem2.CalculateLineTotal();

        order.Items.Add(orderItem1);
        order.Items.Add(orderItem2);

        // Act
        order.CalculateTotal();

        // Assert
        Assert.Equal(100.00m, orderItem1.LineTotal); // 2 * 50.00
        Assert.Equal(75.00m, orderItem2.LineTotal);  // 3 * 25.00
        Assert.Equal(175.00m, order.TotalAmount);    // 100.00 + 75.00
    }

    [Fact]
    public async Task ProductStockManagement_ShouldWorkCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Stock Test Product",
            Description = "Test Description",
            Price = 25.00m,
            StockQuantity = 5,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act - Reduce stock
        product.ReduceStock(2);

        // Assert
        Assert.Equal(3, product.StockQuantity);
        Assert.True(product.HasStock(3));
        Assert.False(product.HasStock(4));
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
