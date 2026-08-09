using Microsoft.EntityFrameworkCore;
using Server.Context;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApiContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();


app.Run();
