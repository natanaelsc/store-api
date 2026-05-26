using Asp.Versioning;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Interfaces;
using Store.Application.Orders.DTOs;
using Store.Domain.Enums;

namespace Store.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Produces("application/json")]
public class OrdersController(
    IOrderService orderService,
    IValidator<CreateOrderRequest> createValidator,
    IValidator<UpdateOrderRequest> updateValidator) : ControllerBase
{

    /// <summary>Cria novo pedido.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await createValidator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        OrderResponse? result = await orderService.CreateOrderAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetOrderById), 
            new { id = result.Id }, 
            result
        );
    }

    /// <summary>Consulta todos os pedidos com filtragem e paginação opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrderListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] Guid? buyerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        OrderFilterRequest filter = new (status, buyerId, page, pageSize);

        OrderListResponse? result = await orderService.GetAllOrdersAsync(filter, cancellationToken);

        return Ok(result);
    }

    /// <summary>Consulta pedido específico por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        OrderResponse? result = await orderService.GetOrderByIdAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>Atualiza os itens de um pedido (somente permitido quando o status é 'Initiated').</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateOrder(
        Guid id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await updateValidator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        OrderResponse? result = await orderService.UpdateOrderAsync(id, request, cancellationToken);

        return Ok(result);
    }

    /// <summary>Exclui um pedido permanentemente.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        await orderService.DeleteOrderAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>Cancela um pedido (permitido quando o status é 'Initiated' ou 'Processed').</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        OrderResponse? result = await orderService.CancelOrderAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>Marca um pedido como processado (permitido quando o status é 'Initiated').</summary>
    [HttpPatch("{id:guid}/process")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ProcessOrder(Guid id, CancellationToken cancellationToken)
    {
        OrderResponse? result = await orderService.ProcessOrderAsync(id, cancellationToken);

        return Ok(result);
    }

    /// <summary>Marca um pedido como enviado (permitido quando o status é 'Processed').</summary>
    [HttpPatch("{id:guid}/ship")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ShipOrder(Guid id, CancellationToken cancellationToken)
    {
        OrderResponse? result = await orderService.ShipOrderAsync(id, cancellationToken);

        return Ok(result);
    }
}
