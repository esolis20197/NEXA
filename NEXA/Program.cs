using Microsoft.EntityFrameworkCore; // Importar Entity Framework Core
using NEXA.Models;
using NEXA.Services;
using Microsoft.EntityFrameworkCore.Migrations;


var builder = WebApplication.CreateBuilder(args);

// Configurar el DbContext con la cadena de conexión desde appsettings.json
builder.Services.AddDbContext<NEXAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NEXA")));
// Habilitar el uso de sesiones
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar sesiones en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Asegura que la cookie de sesión no sea accesible por scripts
    options.Cookie.IsEssential = true; // Necesario para que funcione en GDPR-compliance
});

// EMAIL RECUPERAR CONTRASEÑA
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();

//Agrega la API
builder.Services.AddHttpClient<GometaApiService>();

// Agregar servicios para controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Habilitar HSTS (HTTP Strict Transport Security)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Middleware para usar sesiones

app.UseAuthorization();

// Configurar la ruta predeterminada
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();