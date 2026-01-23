using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestSystem.Components;
using TestSystem.Components.Account;
using TestSystem.Data;
using TestSystem.Infrastructure.Identity;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces;
using TestSystem.ServiceLayer.Services;

namespace TestSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // adding ApplicationDbContext and BusinessDbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // adding BusinessDbContext for main application data - replace with DbContextFactory to resolve issues with a lifetime!!!
            builder.Services.AddDbContext<BusinessDbContext>(options =>
                options.UseSqlServer(connectionString));

            // adding DbContextFactory for BusinessDbContext - alternative approach to resolve lifetime issues with BusinessDbContext
            //builder.Services.AddDbContextFactory<BusinessDbContext>(options =>
            //    options.UseSqlServer(connectionString));

            // adding database exception filter for development
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // adding HttpContextAccessor to access HttpContext in services
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<ITeacherDashboardService, TeacherDashboardService>();
            builder.Services.AddScoped<ITeacherStudentsService, TeacherStudentsService>();
            builder.Services.AddScoped<ITeacherTestsService, TeacherTestsService>();
            builder.Services.AddScoped<ITeacherTestEditorService, TeacherTestEditorService>();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false; // account confirmation doesnt require emails
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddRoles<IdentityRole>() // adding roles to Identity
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope()) // creating possible roles list and defining admin user by default
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { AppRoles.Admin, AppRoles.User, AppRoles.Teacher, AppRoles.Student }; // roles with AppRoles

                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                var adminEmail = "admin@example.com";
                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(admin, "Admin123!");
                    await userManager.AddToRolesAsync(admin, new[] { AppRoles.Admin, AppRoles.User });
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
