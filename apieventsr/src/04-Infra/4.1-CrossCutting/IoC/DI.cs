using apieventsr.Application.Interfaces;
using apieventsr.Application.Services;
using apieventsr.Data;
using apieventsr.Data.Interfaces;
using apieventsr.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace apieventsr.IoC
{
    public static class DI
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton(configuration);

            services.AddDbContext<ProjectContext>(options =>
                    options.UseNpgsql(configuration["CONNECTION_STRING"]));

            // services.Configure<ConfigAccess>(options =>
            // {
            //     options.KeycloakUrl = configuration["KEYCLOAK_URL"];
            //     options.ApiKey = configuration["API_KEY"];

            // });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IEntityRepository, EntityRepository>();
            services.AddScoped<IService, Service>();
            // services.AddScoped<IKeyCloakService, KeyCloakService>();

            // Módulo Eventos
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();

            // Módulo Inscrições
            services.AddScoped<IEventEnrollmentRepository, EventEnrollmentRepository>();
            services.AddScoped<IEventEnrollmentService, EventEnrollmentService>();

            // Módulo Arquivos
            services.AddScoped<IEnrollmentFileRepository, EnrollmentFileRepository>();
            services.AddScoped<IEnrollmentFileService, EnrollmentFileService>();
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        }
    }
}