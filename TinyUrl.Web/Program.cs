using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using System.Diagnostics;
using TinyUrl.Dal.Mongo;
using TinyUrl.Web.Cache;
using TinyUrl.Web.Mongo;
using TinyUrl.Web.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<TinyUrlDatabaseSettings>(
builder.Configuration.GetSection("TinyUrlDatabase"));

builder.Services.AddSingleton<TinyUrlMongoDal>();
builder.Services.AddSingleton<TinyUrlService>();
builder.Services.AddSingleton<CacheManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();


app.MapGet("{ssn:regex(^[a-zA-Z0-9]{{8}}$)}", (HttpContext context, TinyUrlService service) =>
{
    Stopwatch sw = Stopwatch.StartNew();    
    var shortUrl = context.Request.Path.Value.Replace("/", "");
    var longUrl = service.GetRedirectUrl(shortUrl);
    if (longUrl != null)
    {
        string redirectUrl = string.Empty;
        redirectUrl = (!longUrl.StartsWith("http") ? "http://" : string.Empty) + longUrl;

        Debug.WriteLine($"Hit {sw.ElapsedMilliseconds}");
        context.Response.Redirect(redirectUrl);
    }
    else
    {
        context.Response.StatusCode = 404;
    }

});

app.Run();
