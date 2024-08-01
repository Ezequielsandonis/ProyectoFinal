using Microsoft.AspNetCore.Authentication.Cookies;
using TucConnect.Data;
using TucConnect.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n del servicio de contexto de base de datos
builder.Services.AddSingleton(new Contexto(builder.Configuration.GetConnectionString("conexion")));



// Configuraci�n para Sendbird
builder.Services.AddSingleton<ISendBirdServicio>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var sendbirdAppId = config["Sendbird:AppId"];
    var sendbirdApiToken = config["Sendbird:ApiToken"];

    return new SendbirdService(sendbirdAppId, sendbirdApiToken);
});

// Configuraci�n de autenticaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
    });

// Configuraci�n de SignalR
builder.Services.AddSignalR();

// Configuraci�n de controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuraci�n del middleware
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

// Configuraci�n de p�ginas de error para c�digos de estado
app.UseStatusCodePagesWithRedirects("/Home/Index");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Configuraci�n del endpoint de SignalR
    endpoints.MapHub<ChatHub>("/chatHub");
});
app.Run();
