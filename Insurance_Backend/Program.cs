

using CloudinaryDotNet;

using Insurance_Backend.Config;

using Insurance_Backend.Data;

using Insurance_Backend.Profiles;

using Insurance_Backend.Repositories.classes;

using Insurance_Backend.Repositories.Interfaces;

using Insurance_Backend.Services.Classes;

using Insurance_Backend.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;

using Serilog;

using System.Text;

using System.Text.Json.Serialization;

namespace Insurance_Backend

{

    public class Program

    {

        public static void Main(string[] args)

        {

            var builder = WebApplication.CreateBuilder(args);

            // Serilog configuration

            builder.Host.UseSerilog((context, configuration) =>

            {

                configuration.ReadFrom.Configuration(context.Configuration);

            });

            // Controllers and JSON serialization

            builder.Services.AddControllers().AddJsonOptions(options =>

            {

                options.JsonSerializerOptions.ReferenceHandler =

                    ReferenceHandler.IgnoreCycles;

                options.JsonSerializerOptions.Converters.Add(

                    new JsonStringEnumConverter());

            });

            builder.Services.AddEndpointsApiExplorer();

            // Database

            builder.Services.AddDbContext<AppDbContext>(options =>

            {

                options.UseSqlServer(

                    builder.Configuration.GetConnectionString("DatabaseConnection"));

            });

            // Existing repositories

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();

            builder.Services.AddScoped<IClaimRepository, ClaimRepository>();

            builder.Services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();

            // Keshav's additional repositories

            builder.Services.AddScoped<IClaimStatusHistoryRepository, ClaimStatusHistoryRepository>();

            builder.Services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();

            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

            builder.Services.AddScoped<IPolicyPlanRepository, PolicyPlanRepository>();

            builder.Services.AddScoped<IPremiumPaymentRepository, PremiumPaymentRepository>();

            // Token service

            builder.Services.AddScoped<ITokenService, TokenService>();

            // AutoMapper

            builder.Services.AddAutoMapper(cfg =>

            {

                cfg.AddProfile<MappingProfile>();

            });

            // Cloudinary

            builder.Services.Configure<CloudinarySettings>(

                builder.Configuration.GetSection("CloudinarySettings"));

            builder.Services.AddSingleton(provider =>

            {

                var settings = provider

                    .GetRequiredService<IOptions<CloudinarySettings>>()

                    .Value;

                var account = new Account(

                    settings.CloudName,

                    settings.ApiKey,

                    settings.ApiSecret);

                return new Cloudinary(account);

            });

            // JWT settings

            builder.Services.Configure<JWTSettings>(

                builder.Configuration.GetSection("JWTSettings"));

            builder.Services.AddAuthentication(options =>

            {

                options.DefaultAuthenticateScheme =

                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =

                    JwtBearerDefaults.AuthenticationScheme;

            })

            .AddJwtBearer(options =>

            {

                options.TokenValidationParameters =

                    new TokenValidationParameters

                    {

                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["JWTSettings:Issuer"],

                        ValidAudience = builder.Configuration["JWTSettings:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(

                            Encoding.UTF8.GetBytes(

                                builder.Configuration["JWTSettings:SecretKey"]!))

                    };

            });

            // Swagger with JWT bearer authentication

            builder.Services.AddSwaggerGen(options =>

            {

                options.SwaggerDoc("v1", new OpenApiInfo

                {

                    Version = "v1",

                    Title = "Insurance App"

                });

                var securityScheme = new OpenApiSecurityScheme

                {

                    Name = "Authorization",

                    Description = "Enter JWT token like: Bearer {your token}",

                    In = ParameterLocation.Header,

                    Type = SecuritySchemeType.Http,

                    Scheme = "bearer",

                    BearerFormat = "JWT",

                    Reference = new OpenApiReference

                    {

                        Id = "Bearer",

                        Type = ReferenceType.SecurityScheme

                    }

                };

                options.AddSecurityDefinition("Bearer", securityScheme);

                options.AddSecurityRequirement(

                    new OpenApiSecurityRequirement

                    {

                        {

                            securityScheme,

                            Array.Empty<string>()

                        }

                    });

            });

            var app = builder.Build();

            // HTTP request pipeline

            if (app.Environment.IsDevelopment())

            {

                app.UseSwagger();

                app.UseSwaggerUI();

            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }

    }

}

