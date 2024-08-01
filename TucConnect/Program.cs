using Microsoft.AspNetCore.Authentication.Cookies;
using TucConnect.Data;
using TucConnect.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuración del servicio de contexto de base de datos
builder.Services.AddSingleton(new Contexto(builder.Configuration.GetConnectionString("conexion")));



// Configuración para Sendbird
builder.Services.AddSingleton<ISendBirdServicio>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var sendbirdAppId = config["Sendbird:AppId"];
    var sendbirdApiToken = config["Sendbird:ApiToken"];

    return new SendbirdService(sendbirdAppId, sendbirdApiToken);
});

// Configuración de autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
    });

// Configuración de SignalR
builder.Services.AddSignalR();

// Configuración de controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configuración de páginas de error para códigos de estado
app.UseStatusCodePagesWithRedirects("/Home/Index");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Configuración del endpoint de SignalR
    endpoints.MapHub<ChatHub>("/chatHub");
});
app.Run();
