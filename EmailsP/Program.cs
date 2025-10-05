using Application.Services;
using Domain.Interfaces;
using Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Text.Json;

// ====== Manejadores globales para que el host no muera por excepciones en hilos de fondo (DEV) ======
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    try
    {
        var ex = e.ExceptionObject as Exception;
        Console.WriteLine($"[FATAL][AppDomain] {ex}");
    }
    catch { }
};
TaskScheduler.UnobservedTaskException += (s, e) =>
{
    try
    {
        Console.WriteLine($"[ERROR][UnobservedTask] {e.Exception}");
        e.SetObserved();
    }
    catch { }
};

var builder = WebApplication.CreateBuilder(args);

// ---------- Controllers + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Emails API", Version = "v1" });

    var jwt = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Pega SOLO el token (Swagger agregará 'Bearer ').",
        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
    };

    options.AddSecurityDefinition(jwt.Reference.Id, jwt);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwt, Array.Empty<string>() } });
});

// ---------- Límites de subida (adjuntos) ----------
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    o.ValueLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 100L * 1024L * 1024L; // 100 MB
});
builder.Services.Configure<IISServerOptions>(o =>
{
    o.MaxRequestBodySize = 100L * 1024L * 1024L;
});

// ---------- CORS (útil si pruebas desde otro origen distinto al propio host) ----------
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DevAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ---------- JWT ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "clave-secreta-larga-por-defecto");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // solo DEV/local
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ---------- DI (Infraestructura + Aplicación) ----------
// Email
builder.Services.AddScoped<IEmailService, GmailSenderService>();     // Envío real
// builder.Services.AddScoped<IEmailService, NullEmailService>();    // ← alterna para diagnóstico si lo necesitas
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<EmailSenderUseCase>();

// Contactos (persistencia JSON)
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ContactService>();

// Auth
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevAll");
}

// ---------- Middleware global de excepciones (responde 500 JSON en vez de cerrar conexión) ----------
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var payload = new { title = "Error inesperado", detail = ex?.Message, status = 500 };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    });
});

app.UseHttpsRedirection();

// ---------- Archivos estáticos (wwwroot) para la mini UI /compose.html ----------
app.UseStaticFiles();

// (Opcional) redirigir raíz a la UI de envío
app.MapGet("/", ctx => { ctx.Response.Redirect("/compose.html"); return Task.CompletedTask; });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
