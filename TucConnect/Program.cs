using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using TucConnect.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(new Contexto(builder.Configuration.GetConnectionString("conexion")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    option.LoginPath = "/Cuenta/Login";
});
//SignalIr para el chat
builder.Services.AddSignalR();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseStatusCodePagesWithRedirects("~/Home/Index");


//ENDPOINTS
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
    //   endpoints.MapHub<ChatHub>("/chat");
});

app.Run();



//RECORDAR INSTALAR BIBLIOTECA DE SSIGNALR DEL LADO DEL CLIENTE
//@microsoft/signalr@latest

//contraseña de soome d$cxmv4TKBjf7#+