using Microsoft.AspNetCore.Builder;
using TiendaWed.Models;
using TiendaWed.Repositorio;


var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IRepositorioUsuario, RepositorioUsuario>();
//builder.Services.AddTransient<IRepositorioInventario, RepositorioInventario>();
builder.Services.AddTransient<IRepositorioPDF, RepositorioPDF>();
builder.Services.AddTransient<IRepositorioPedido, RepositorioPedido>();

builder.Services.AddTransient<IRepositorioProducto, RepositorioProducto>();
//builder.Services.AddTransient<IRepositorioContacto, RepositorioContactano>();
builder.Services.AddTransient<ICarritoServicio, carritoServicio>();
builder.Services.AddTransient<productoSelecionados>();
//builder.Services.AddTransient<IRepositorioProveedor, RepositorioProveedor>();
//builder.Services.AddTransient<IRepositorioConsulta, ReposirorioConsulta>();



builder.Services.AddHttpContextAccessor();//agrega el acceso a Http context
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});





// Add services to the container.


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
app.UseSession();
app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Logins}/{action=Logins}/{id?}");

app.Run();
