using BuildingBlocks.Application.Common.Interfaces;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Infrastructure.Data;
using BuildingBlocks.Infrastructure.Data.Interceptors;
using BuildingBlocks.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("GymAppDb");
        Guard.Against.Null(connectionString, message: "Connection string 'GymAppDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));

            options.AddPolicy(Policies.CanPrepareOffer, policy => policy.RequireRole(Roles.SalesSpecialist));
            options.AddPolicy(Policies.CanReviewOffer, policy => policy.RequireRole(Roles.SalesCoordinator));
            options.AddPolicy(Policies.CanRejectOrApproveOffer, policy => policy.RequireRole(Roles.SalesCoordinator));
            options.AddPolicy(Policies.CanCorrectOffer, policy => policy.RequireRole(Roles.SalesSpecialist));
            options.AddPolicy(Policies.CanPublishOffer, policy => policy.RequireRole(Roles.SalesSpecialist));
        });
    }
}
