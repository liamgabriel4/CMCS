using CMCS.Data; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args); // Create a builder for the web application

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); // Retrieve the connection string from the configuration

// Add Entity Framework Core database context with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Configure the context to use SQL Server with the retrieved connection string

builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Add a developer exception filter for database-related errors

// Configure default identity with user and role management
builder.Services.AddDefaultIdentity<IdentityUser>() // Use the default identity user
    .AddDefaultTokenProviders() // Add default token providers for user authentication
    .AddRoles<IdentityRole>() // Enable role management
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Use Entity Framework Core for storing user and role data

var app = builder.Build(); // Build the web application

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) // Check if the application is in the development environment
{
    app.UseMigrationsEndPoint(); // Enable migrations endpoint for development
}
else // If not in development
{
    app.UseExceptionHandler("/Home/Error"); // Use a generic error handler
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts(); // Enable HTTP Strict Transport Security (HSTS)
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files (e.g., CSS, JavaScript, images)

app.UseRouting(); // Enable routing for the application

app.UseAuthorization(); // Enable authorization middleware

// Define the default route for the application
app.MapControllerRoute(
    name: "default", // Name of the route
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Default route pattern with optional id parameter

app.MapRazorPages(); // Map Razor Pages in the application

app.Run(); // Run the web application

//Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
//Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].