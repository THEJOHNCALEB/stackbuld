using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stackbuld.ProductOrdering.Application.DTOs;
using Stackbuld.ProductOrdering.Application.Orders.Commands;
using Stackbuld.ProductOrdering.Application.Orders.Queries;

namespace Stackbuld.ProductOrdering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderDto order)
    {
        var createdOrder = await _mediator.Send(new CreateOrderCommand(order));
        return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
    }
}
