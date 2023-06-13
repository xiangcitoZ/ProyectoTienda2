using Amazon.S3;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;
using ProyectoTienda2.Data;
using ProyectoTienda2.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("MySqlProyectoTienda");

string secrets =
            HelperSecretManager.GetSecretAsync().Result;
KeysModel model = JsonConvert.DeserializeObject<KeysModel>(secrets);

builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddTransient<ServiceApi>();
builder.Services.AddTransient<ServiceStorageS3>();
builder.Services.AddTransient<ServiceAwsCache>();


builder.Services.AddDbContext<ProyectoTiendaContext>
    (options => options.UseMySql(model.MySqlTienda
    , ServerVersion.AutoDetect(model.MySqlTienda)));


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "cache-proyecto-tienda.1xwnbt.ng.0001.use1.cache.amazonaws.com:6379";
    options.InstanceName = "cache-proyecto-tienda";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddAntiforgery();
builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false);

//SEGURIDAD
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(
    CookieAuthenticationDefaults.AuthenticationScheme,
    config =>
    {
        config.AccessDeniedPath = "/Cliente/ErrorAccesoCliente";
    });

builder.Services.AddControllersWithViews(options =>
options.EnableEndpointRouting = false)
    .AddSessionStateTempDataProvider();

builder.Services.AddHttpContextAccessor();

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

app.UseSession();

app.UseMvc(route =>
{
    route.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();