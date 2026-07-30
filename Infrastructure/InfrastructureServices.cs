using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Auth.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.InventoryTransaction.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.Interfaces;
using Application.Features.Product.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seed;
using Infrastructure.Security.Hashing;
using Infrastructure.Security.Jwt;
using Infrastructure.Services.Payment.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ShopDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("local")));
        services.AddScoped<HttpClient>();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddScoped<UnitOfWorkContract, UnitOfWork>();
        services.AddScoped<OrderRepositoryContract, OrderRepository>();
        services.AddScoped<ProductRepositoryContract, ProductRepository>();
        services.AddScoped<PaymentRepositoryContract, PaymentRepository>();
        services.AddScoped<PaymentGatewayResolverContract, PaymentGatewayResolver>();
        services.AddScoped<PaymentGatewayProviderContract, ZarinPalPaymentGatewayProvider>();
        services.AddScoped<PaymentGatewayProviderContract, SamanPaymentGatewayProvider>();
        services.AddScoped<CartRepositoryContract, CartRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUSerContext, UserContext>();
        services.AddScoped<InventoryRepositoryContract, InventoryRepository>();
        services.AddScoped<InventoryTransactionRepositoryContract, InventoryTransactionRepository>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}