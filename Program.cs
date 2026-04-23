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

// Detectar si estamos corriendo en un contenedor Docker/Render
// En contenedor, el WORKDIR es /app, así que siempre usamos esa ruta explícitamente
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
               || Environment.GetEnvironmentVariable("RENDER") != null;

var connectionString = isDocker
    ? "Data Source=/app/integrador.db"
    : (builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=integrador.db");

Console.WriteLine($"[DB] Connection string: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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

// Crear/migrar la base de datos al arrancar la aplicación
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Intentar aplicar migraciones primero
    context.Database.Migrate();
    Console.WriteLine("[DB] Migraciones aplicadas exitosamente.");
}
catch (Exception migrateEx)
{
    Console.WriteLine($"[DB] Migrate() falló: {migrateEx.Message}. Intentando EnsureCreated()...");
    try
    {
        using var scope2 = app.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context2.Database.EnsureCreated();
        Console.WriteLine("[DB] EnsureCreated() ejecutado exitosamente.");
    }
    catch (Exception ensureEx)
    {
        // Log crítico pero no detener la app — el health check seguirá respondiendo
        Console.WriteLine($"[DB] CRÍTICO - EnsureCreated() también falló: {ensureEx.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

app.Run();
