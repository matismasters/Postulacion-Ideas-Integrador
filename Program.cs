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


var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"[DB] DATABASE_URL existe: {!string.IsNullOrEmpty(databaseUrl)}");

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

// ── Base de datos: PostgreSQL ─────────────────────────────────────────────────
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl))
{
    // En producción (Render): usar la URI de PostgreSQL directamente
    connectionString = databaseUrl;
    Console.WriteLine("[DB] Usando DATABASE_URL de variable de entorno (PostgreSQL en Render).");
}
else
{
    // En desarrollo local: usar appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Database=integradorideas;Username=postgres;Password=postgres";
    Console.WriteLine($"[DB] Usando connection string local: {connectionString}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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

    // Aplicar migraciones de forma segura
    context.Database.Migrate();
    Console.WriteLine("[DB] Migraciones aplicadas exitosamente.");
}
catch (Exception migrateEx)
{
    Console.WriteLine($"[DB] ERROR CRÍTICO - Migrate() falló: {migrateEx.Message}. Asegúrese de que la base de datos PostgreSQL esté en ejecución.");
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
