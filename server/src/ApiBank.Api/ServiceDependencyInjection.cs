using ApiBank.Infrastructure.ApiServices;

namespace ApiBank.Api;

public static class ServiceDependencyInjection
{
    public static void AddApiBankServices(this IServiceCollection services)
    {
        services.AddScoped<ApiServiceA>();
        
        
    }
}