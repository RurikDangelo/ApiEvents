using apieventsr.IoC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace apieventsr.Api
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder => builder
                                      .WithOrigins(
                                        "https://pmais-development.p4ed.com"
                                      )
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            services.AddControllers();
            services.AddServices(configuration);

            services.AddSwaggerGen(settings =>
            {
                settings.SwaggerDoc("v1", new OpenApiInfo { Version = "v1" });
                settings.EnableAnnotations();

                // Botão "Authorize" no Swagger para colar o Bearer token
                settings.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Cole o JWT token. Em Development use o token gerado em jwt.io (veja GUIA-DESENVOLVEDOR.md)"
                });
                settings.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    if (webHostEnvironment.IsDevelopment())
                    {
                        // Bypass para testes locais sem Keycloak.
                        // Gere um token em https://jwt.io com alg=HS256
                        // e secret: "dev-secret-key-only-for-local-testing"
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateLifetime = false,
                            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes("dev-secret-key-only-for-local-testing"))
                        };
                    }
                    else
                    {
                        options.Authority = configuration["AUTHORITY"];
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = true,
                            ValidIssuer = configuration["ISSUER"],
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    }
                });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddHealthChecks();

            return services;
        }

        public static WebApplication Configure(this WebApplication app)
        {
            app.UseCors("AllowSpecificOrigins");

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "Poliedro - Example API";
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ControllerHandler.ControllerHandler>();
            app.MapHealthChecks("/api/health-check");

            app.MapControllers();

            return app;
        }
    }
}
