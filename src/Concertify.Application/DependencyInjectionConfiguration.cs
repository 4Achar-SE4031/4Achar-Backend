using Microsoft.Extensions.DependencyInjection;

using Concertify.Domain.Interfaces;
using Concertify.Infrastructure.ExternalServices;
using Concertify.Application.Services;


namespace Concertify.Application;

public class DependencyInjectionConfiguration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DtoEntityMapperProfile));

        services.AddScoped<IAccountService, AccountService>();
    }
        
}
