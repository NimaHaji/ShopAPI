using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed;
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

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}