﻿using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);
        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }
        var salesSpecialistRole = new IdentityRole(Roles.SalesSpecialist);
        if (_roleManager.Roles.All(r => r.Name != salesSpecialistRole.Name))
        {
            await _roleManager.CreateAsync(salesSpecialistRole);
        }
        var salesCoordinatorRole = new IdentityRole(Roles.SalesCoordinator);
        if (_roleManager.Roles.All(r => r.Name != salesCoordinatorRole.Name))
        {
            await _roleManager.CreateAsync(salesCoordinatorRole);
        }
        var customerRole = new IdentityRole(Roles.Customer);
        if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
        {
            await _roleManager.CreateAsync(customerRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator", Email = "administrator@example.com" };
        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name)
                && !string.IsNullOrWhiteSpace(salesSpecialistRole.Name)
                && !string.IsNullOrWhiteSpace(salesCoordinatorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] 
                { 
                    administratorRole.Name, salesSpecialistRole.Name, salesCoordinatorRole.Name
                });
            }
        }
        var exampleCustomer = new ApplicationUser { UserName = "customer", Email = "customer@example.com" };
        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(exampleCustomer, "Customer1!");
            if (!string.IsNullOrWhiteSpace(customerRole.Name))
            {
                await _userManager.AddToRolesAsync(exampleCustomer, new[] { customerRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Todo List",
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }
    }
}
