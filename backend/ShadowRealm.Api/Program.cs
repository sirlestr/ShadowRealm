using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using ShadowRealm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ShadowRealm API", Version = "v1" });

    // Definice JWT schématu
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Zadej 'Bearer {token}' do pole níže."
    });

    // Aplikuj JWT schéma na všechny endpointy
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

//registrace tokenservice
builder.Services.AddScoped<TokenService>();

//set url 
builder.WebHost.UseUrls("http://localhost:5000");


//načtení konfigurace JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));


//add db connection 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));




// autentizace 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true
        };
    });
//aktivace kontrololeru
builder.Services.AddControllers();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IQuestService, QuestService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); 

// Zde později přidáme autentizaci, routování, controllery, atd.
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
logger.LogInformation("JWT Issuer from config: {Issuer}", jwtIssuer);


app.MapGet("/ping", (ILogger<Program> logger) =>
{
    logger.LogInformation("Ping endpoint was called at {Time}", DateTime.UtcNow);
    return Results.Ok(new { message = "pong" });
});


//centralizoavné error handling
app.UseExceptionHandler(errorApp =>
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        logger.LogError(exception, "Unhandled exception occurred.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "Internal server error." });

    }));


// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Seed initial data
    DbSeeder.SeedInitialData(db);
    logger.LogInformation("Database seeded with initial data.");
}

app.Run();