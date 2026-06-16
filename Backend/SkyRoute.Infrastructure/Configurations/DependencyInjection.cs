using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SkyRoute.Application.DTOs.Auth;
using SkyRoute.Application.DTOs.Flight;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Application.Validators;
using SkyRoute.Infrastructure.Authentication;
using SkyRoute.Infrastructure.Data;
using SkyRoute.Infrastructure.Data.Repositories;
using SkyRoute.Infrastructure.Pricing;
using System.Text;

namespace SkyRoute.Infrastructure.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<SkyRouteDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();

        // Pricing Strategies (Strategy Pattern)
        services.AddSingleton<IFlightPricingStrategy, GlobalAirPricingStrategy>();
        services.AddSingleton<IFlightPricingStrategy, BudgetWingsPricingStrategy>();

        // Authentication
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAirportService, AirportService>();
        services.AddScoped<IFlightSearchService, FlightSearchService>();
        services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
        services.AddScoped<IValidator<FlightSearchRequestDto>, FlightSearchValidator>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer           = true,
                ValidIssuer              = jwtSettings.Issuer,
                ValidateAudience         = true,
                ValidAudience            = jwtSettings.Audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };
        });

        return services;
    }
}
