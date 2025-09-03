using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Products.Commands;
using Stackbuld.ProductOrdering.Application.Products.Queries;

namespace Stackbuld.ProductOrdering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto product)
    {
        var createdProduct = await _mediator.Send(new CreateProductCommand(product));
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto product)
    {
        try
        {
            var updatedProduct = await _mediator.Send(new UpdateProductCommand(id, product));
            return Ok(updatedProduct);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand(id));
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
