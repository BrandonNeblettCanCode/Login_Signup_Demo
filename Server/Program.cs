using Microsoft.EntityFrameworkCore;
using Server.Context;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApiContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5); //
        }
    );
});
builder.Services.AddCors(options => 
{
    options.AddPolicy("allowFrontend", policy => 
    {
        policy.WithOrigins(
        "http://localhost:5039", "http://localhost:5083")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.UseCors("allowFrontend");
app.UseRouting();

app.Run();
