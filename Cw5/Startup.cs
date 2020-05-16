using Cw5.Middlewares;
using Cw5.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Cw5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IStudentDbService, SqlServerStudentDbService>();
            services.AddScoped<IStudentsDal, SqlServerDbDal>();
            services.AddControllers();

            //dodawanie dokumentacji (1/2)
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Students App API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentDbService service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Middleware obs³uguj¹cy b³êdy
            app.UseMiddleware<ExceptionMiddleware>();

            //dodawanie dokumentacji (2/2)
            app.UseSwagger();
            app.UseSwaggerUI(config => 
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API");
            });

            //Middleware loguj¹cy ¿¹dania
            app.UseMiddleware<LoggingMiddleware>();

            //Middleware - Index: s***** -> db

            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index")) 
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Nale¿y podaæ numer indexu");
                    return; //short circuit, od razu zwracana odpowiedŸ, nie przekazywana do kolejnego Middleware
                }
                string index = context.Request.Headers["Index"].ToString();
                bool studentExists = service.StudentExists(index);

                if (!studentExists)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Student " + index + " nie istnieje, brak autoryzacji");
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status202Accepted;
                await next();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
