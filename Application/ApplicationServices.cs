using Application.Features.Address.Implementations;
using Application.Features.Address.Interfaces;
using Application.Features.Auth.Interfaces;
using Application.Features.Auth.Services;
using Application.Features.Cart.implementations;
using Application.Features.Cart.Interfaces;
using Application.Features.Checkout.Implement;
using Application.Features.Checkout.Interfaces;
using Application.Features.Coupon.Implementations;
using Application.Features.Coupon.Interfaces;
using Application.Features.Discount.Implementations;
using Application.Features.Discount.Interfaces;
using Application.Features.Home.Implementations;
using Application.Features.Home.Interfaces;
using Application.Features.IdempotencyKey.Implementations;
using Application.Features.IdempotencyKey.Interfaces;
using Application.Features.Inventory.Implement;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.implementations;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.Interfaces;
using Application.Features.Payment.Services;
using Application.Features.Product.implementations;
using Application.Features.Product.Interfaces;
using Application.Features.Review.Implement;
using Application.Features.Review.interfaces;
using Application.Features.Wishlist.Implementations;
using Application.Features.Wishlist.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PaymentServiceContract,PaymentService>();
        services.AddScoped<ProductServicesContract,ProductService>();
        services.AddScoped<OrderServicesContract,OrderService>();
        services.AddScoped<CartServicesContract,CartService>();
        services.AddScoped<IUserService,UserService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<CheckoutServiceContract,CheckoutService>();
        services.AddScoped<InventoryServiceContract, InventoryService>();
        services.AddScoped<ReviewServiceContract, ReviewService>();
        services.AddScoped<WishlistServiceContract, WishlistService>();
        services.AddScoped<DiscountServiceContract, DiscountService>();
        services.AddScoped<CouponsServiceContract, CouponService>();
        services.AddScoped<AddressServiceContract, AddressService>();
        services.AddScoped<HomeServiceContract, HomeService>();
        services.AddScoped<IdempotencyServiceContract, IdempotencyService>();
        return services;
    }
}