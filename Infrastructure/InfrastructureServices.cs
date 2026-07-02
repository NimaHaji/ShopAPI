using Application.Features.Order.Interfaces;
using Application.Features.Payment.Interfaces;
using Application.Features.Product.Interfaces;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seed;
using Infrastructure.Services.Implement;
using Infrastructure.Services.Payment.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShopDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("local")));

        services.AddHttpClient();
        services.AddScoped<OrderRepositoryContract,OrderRepository>();
        services.AddScoped<ProductRepositoryContract,ProductRepository>();
        services.AddScoped<PaymentRepositoryContract,PaymentRepository>();
        services.AddScoped<PaymentGatewayResolverContract,PaymentGatewayResolver>();
        services.AddScoped<PaymentGatewayProviderContract,ZarinPalPaymentGatewayProvider>();
        services.AddScoped<PaymentGatewayProviderContract,SamanPaymentGatewayProvider>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}