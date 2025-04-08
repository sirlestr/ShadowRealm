using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=shadowrealm.db"));

// Zde později přidáme:
// - DbContext
// - Autentizaci (JWT)
// - Služby

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Zde později přidáme autentizaci, routování, controllery, atd.

app.Run();