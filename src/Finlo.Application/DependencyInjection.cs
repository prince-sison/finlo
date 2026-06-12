using Finlo.Application.Interfaces.Services;
using Finlo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Finlo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}