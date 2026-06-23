using Microsoft.EntityFrameworkCore;
using ShiftSync.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Allow angular app to access the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Replace with the actual URL of your Angular app
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Registering the DbContext with the SQLite provider and connection string from appsettings.json

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
