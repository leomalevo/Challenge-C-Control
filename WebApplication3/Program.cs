using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 

//Enable CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

// Configure In-Memory Database (Can be changed to SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AuthDB"));

// Add Identity for Authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure IdentityServer for Authorization
builder.Services.AddIdentityServer()
    .AddInMemoryClients(IdentityServerConfig.Clients)
    .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
    .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
    .AddAspNetIdentity<ApplicationUser>();

// Configure Authentication & Authorization
builder.Services.AddAuthentication()
    .AddIdentityServerJwt(); // 

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Enable Swagger for API Documentation
builder.Services.AddEndpointsApiExplorer();

// Define the file for Swagger documentation
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    options.IncludeXmlComments(xmlPath);
    
});

var app = builder.Build();

// Creates user by default
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var adminRole = "Admin";
    var adminUser = "admin";
    var adminPassword = "Admin@123";

    // assign admin rol
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // create admin user
    var user = await userManager.FindByNameAsync(adminUser);
    if (user == null)
    {
        var newUser = new ApplicationUser { UserName = adminUser, Email = "admin@example.com" };
        var result = await userManager.CreateAsync(newUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, adminRole);
            Console.WriteLine("✅ Usuario Admin creado correctamente.");
        }
        else
        {
            Console.WriteLine($"❌ Error al crear Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}

// CORS Policy Before Routing
app.UseCors("AllowAngular");

// Enable WebSockets Middleware
app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await WebSocketHandler.HandleConnection(webSocket);
    }
    else
    {
        await next();
    }
});

// Configure Routing, Authentication & Authorization Middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.Run();