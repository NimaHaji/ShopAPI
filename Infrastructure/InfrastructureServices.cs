using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Address.Interfaces;
using Application.Features.Auth.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.CartItem.Interfaces;
using Application.Features.Coupon.Interfaces;
using Application.Features.CouponUsage.Interfaces;
using Application.Features.Discount.Interfaces;
using Application.Features.DiscountProduct.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.InventoryTransaction.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.Interfaces;
using Application.Features.Product.Interfaces;
using Application.Features.Review.interfaces;
using Application.Features.Wishlist.Interfaces;
using Domain.Services;
using Infrastructure.Email;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seed;
using Infrastructure.Security.Hashing;
using Infrastructure.Security.Jwt;
using Infrastructure.Security.Verification;
using Infrastructure.Services.Payment.Implement;
using Infrastructure.Services.Sku;
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
        services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<IUSerContext, UserContext>();
        services.AddScoped<InventoryRepositoryContract, InventoryRepository>();
        services.AddScoped<InventoryTransactionRepositoryContract, InventoryTransactionRepository>();
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IVerificationCodeGenerator, VerificationCodeGenerator>();
        services.AddScoped<SkuGeneratorContract, SkuGenerator>();
        services.AddScoped<ReviewsRepositoryContract, ReviewsRepository>();
        services.AddScoped<CartItemRepositoryContract, CartItemRepository>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<WishlistRepositoryContract, WishlistRepository>();
        services.AddScoped<WishlistItemRepositoryContract, WishlistItemRepository>();
        services.AddScoped<DiscountRepositoryContract, DiscountRepository>();
        services.AddScoped<DiscountProductRepositoryContract, DiscountProductRepository>();
        services.AddScoped<CouponRepositoryContract, CouponRepository>();
        services.AddScoped<CouponUsageRepositoryContract, CouponUsageRepository>();
        services.AddScoped<AddressRepositoryContract, AddressRepository>();
        
        return services;
    }
}