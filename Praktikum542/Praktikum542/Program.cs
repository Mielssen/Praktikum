using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Praktikum542.Exceptions;
using Praktikum542.Middleware;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Praktikum542 API",
            Version = "v1",
            Description = "API для сервісу бронювання турів TravelManager"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Введіть JWT токен (без префікса 'Bearer')"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod());
    });

    builder.Services.AddDbContext<PraktikumContext>(options =>
    {
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 0))
        );
    });

    builder.Services.Configure<IISServerOptions>(options =>
    {
        options.MaxRequestBodySize = 500_000_000;
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 500_000_000;
    });

    builder.Services.AddScoped<SavedPersonRepository>();
    builder.Services.AddScoped<SavedPersonService>();
    builder.Services.AddScoped<FavoriteRepository>();
    builder.Services.AddScoped<FavoriteService>();
    builder.Services.AddScoped<BookingRepository>();
    builder.Services.AddScoped<BookingService>();
    builder.Services.AddScoped<CredentialsRepository>();
    builder.Services.AddScoped<AuthentificationService>();
    builder.Services.AddScoped<TourRepository>();
    builder.Services.AddScoped<TourService>();
    builder.Services.AddScoped<AdminRepository>();
    builder.Services.AddScoped<AdminService>();
    builder.Services.AddScoped<PasswordResetRepository>();
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();

    var jwtSettings = builder.Configuration.GetSection("Jwt");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            if (exception is AppException appEx)
            {
                context.Response.StatusCode = 400;
                logger.LogWarning("AppException: Code={Code}, Message={Message}", appEx.Code, appEx.Message);
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Message = appEx.Message,
                    Code = appEx.Code
                });
            }
            else
            {
                context.Response.StatusCode = 500;
                logger.LogError(exception, "Необроблена помилка сервера");
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Message = "Internal server error"
                });
            }
        });
    });

    app.UseDefaultFiles();
    app.UseStaticFiles();

    var uiPath = Path.Combine(builder.Environment.ContentRootPath, "TravelManagerUI");
    if (Directory.Exists(uiPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uiPath),
            RequestPath = ""
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Praktikum542 API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Додаток неочікувано завершив роботу");
}
finally
{
    Log.CloseAndFlush();
}