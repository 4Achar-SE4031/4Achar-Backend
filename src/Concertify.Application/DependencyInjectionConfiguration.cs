using Concertify.Application.Services;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;
using Concertify.Infrastructure.Data;
using Concertify.Infrastructure.ExternalServices;
using Concertify.Scraper.Scrapers;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


namespace Concertify.Application;

public class DependencyInjectionConfiguration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DtoEntityMapperProfile));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IGenericRepository<Concert>, GenericRepository<Concert>>();
        
        services.AddScoped<IScraperService, ScraperService>();
        services.AddScoped<IWebScraper, HonarTicketScraper>();
        services.AddScoped<IWebDriver, ChromeDriver>(sp =>
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--ingore-ssl-errors=yes",
                "--ignore-certificate-errors",
                "--no-sandbox",
                "--headless");
            return ActivatorUtilities.CreateInstance<ChromeDriver>(sp, options);
        });
    }
        
}
