using Microsoft.EntityFrameworkCore;
using Stackbuld.ProductOrdering.Application.Interfaces;
using Stackbuld.ProductOrdering.Domain.Entities;
using Stackbuld.ProductOrdering.Infrastructure.Data;

namespace Stackbuld.ProductOrdering.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> TryReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            "UPDATE \"Products\" SET \"StockQuantity\" = \"StockQuantity\" - {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2} AND \"StockQuantity\" >= {0}",
            quantity, DateTime.UtcNow, productId);

        return rowsAffected > 0;
    }
}
