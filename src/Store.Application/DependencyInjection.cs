using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Store.Application.Interfaces;
using Store.Application.Orders;

namespace Store.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        return services;
    }
}
