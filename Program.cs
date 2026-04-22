using IntegradorIdeas.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using IntegradorIdeas.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configuración para Render: usar el puerto proporcionado por el entorno
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar Health Checks
builder.Services.AddHealthChecks();

// Configurar Forwarded Headers para Render (proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Professor/Login";
        options.AccessDeniedPath = "/Home/Index";
    });

builder.Services.AddScoped<IIdeaSimilarityService, IdeaSimilarityService>();

var app = builder.Build();

// Mostrar errores detallados en todos los entornos para depuración inicial en Render
app.UseDeveloperExceptionPage();

// Apply pending migrations on startup to create the DB in Docker/Render
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Base de datos migrada exitosamente.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error al migrar la base de datos: {ex.Message}");
    // No detenemos la app para que al menos el health check responda
}

// Configure the HTTP request pipeline.
if (false && !app.Environment.IsDevelopment()) // Desactivado temporalmente para ver el error
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();

// Deshabilitado para evitar bucles de redirección en Render (Render ya maneja HTTPS)
// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health");

app.Run();
