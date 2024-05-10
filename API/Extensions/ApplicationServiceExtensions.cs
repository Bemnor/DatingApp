using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddCors();

        //Provide ITokenService interface. using AddScoped() makes sure itll be garbageCollected whenever its controller is disposed of at the end of an http request.
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
