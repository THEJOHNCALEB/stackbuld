using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Stackbuld.ProductOrdering.Domain.Exceptions;
using System.Net;

namespace Stackbuld.ProductOrdering.Api.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception occurred");

        var response = context.Exception switch
        {
            InsufficientStockException ex => new ObjectResult(new
            {
                error = "Insufficient stock",
                message = ex.Message,
                productId = ex.ProductId,
                productName = ex.ProductName,
                availableStock = ex.AvailableStock,
                requestedQuantity = ex.RequestedQuantity
            })
            {
                StatusCode = (int)HttpStatusCode.Conflict
            },
            ArgumentException ex => new ObjectResult(new
            {
                error = "Invalid argument",
                message = ex.Message
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            },
            InvalidOperationException ex => new ObjectResult(new
            {
                error = "Invalid operation",
                message = ex.Message
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            },
            _ => new ObjectResult(new
            {
                error = "Internal server error",
                message = "An unexpected error occurred"
            })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            }
        };

        context.Result = response;
        context.ExceptionHandled = true;
    }
}
