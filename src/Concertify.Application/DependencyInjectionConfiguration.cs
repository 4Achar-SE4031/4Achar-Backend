using Concertify.Application.Services;
using Concertify.Domain.Interfaces;
using Concertify.Infrastructure.ExternalServices;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Concertify.Application;

public class DependencyInjectionConfiguration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DtoEntityMapperProfile));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailSender, EmailSender>();
    }
        
}
