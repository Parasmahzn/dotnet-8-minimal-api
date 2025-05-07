namespace dotnet.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        // services.AddScoped<IUserService, UserService>();

        return services;
    }
}