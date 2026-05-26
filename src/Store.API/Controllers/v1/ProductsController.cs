using Asp.Versioning;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Orders.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductRequest> validator) : ControllerBase
{

    /// <summary>Cria novo produto.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        Product product = new(request.Name, request.Price);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetProduct), 
            new { id = product.Id }, 
            new { product.Id, product.Name, product.Price, product.CreatedAt }
        );
    }

    /// <summary>Consulta produto por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return NotFound(new { message = $"Product '{id}' not found." });

        return Ok(new { product.Id, product.Name, product.Price, product.CreatedAt });
    }
}
