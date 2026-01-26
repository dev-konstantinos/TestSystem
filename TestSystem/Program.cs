using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestSystem.Components;
using TestSystem.Components.Account;
using TestSystem.Data;
using TestSystem.Infrastructure.Identity;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;
using TestSystem.ServiceLayer.Interfaces.Teacher;
using TestSystem.ServiceLayer.Interfaces.Admin;
using TestSystem.ServiceLayer.Services.Student;
using TestSystem.ServiceLayer.Services.Teacher;
using TestSystem.ServiceLayer.Services.Admin;

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
            //builder.Services.AddDbContext<BusinessDbContext>(options =>
            //    options.UseSqlServer(connectionString));

            // adding DbContextFactory for BusinessDbContext - alternative approach to resolve lifetime issues with BusinessDbContext
            builder.Services.AddDbContextFactory<BusinessDbContext>(options =>
                options.UseSqlServer(connectionString));

            // adding database exception filter for development
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // adding HttpContextAccessor to access HttpContext in services
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<ITeacherDashboardService, TeacherDashboardService>();
            builder.Services.AddScoped<ITeacherStudentsService, TeacherStudentsService>();
            builder.Services.AddScoped<ITeacherTestsService, TeacherTestsService>();
            builder.Services.AddScoped<ITeacherTestEditorService, TeacherTestEditorService>();
            builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
            builder.Services.AddScoped<IStudentTeachersService, StudentTeachersService>();
            builder.Services.AddScoped<IStudentTestsService, StudentTestsService>();
            builder.Services.AddScoped<IStudentResultsService, StudentResultsService>();
            builder.Services.AddScoped<IStudentTestPassingService, StudentTestPassingService>();
            builder.Services.AddScoped<ITeacherResultsService, TeacherResultsService>();


            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true; // account confirmation doesnt require emails
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddRoles<IdentityRole>() // adding roles to Identity
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { AppRoles.Admin, AppRoles.User, AppRoles.Teacher, AppRoles.Student }; // possible roles

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // reading admin data from appsettings.json
                var adminSection = builder.Configuration.GetSection("AdminSeed");

                var adminEmail = adminSection["Email"];
                var adminPassword = adminSection["Password"];
                var confirmEmail = bool.TryParse(adminSection["ConfirmEmail"], out var c) && c;

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                    throw new InvalidOperationException("AdminSeed configuration is missing");

                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = confirmEmail
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(
                            "Failed to create admin: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));

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
