using System.ComponentModel.Design;
using Application.Features.Order.implementations;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.Interfaces;
using Application.Features.Payment.Services;
using Application.Features.Product.implementations;
using Application.Features.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PaymentServiceContract,PaymentService>();
        services.AddScoped<ProductServicesContract,ProductService>();
        services.AddScoped<OrderServicesContract,OrderService>();
        return services;
    }
}