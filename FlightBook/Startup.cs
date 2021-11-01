using FlightBook.DomainModel;
using FlightBook.Persistence.EFCore;
using FlightBook.ServiceInterfaces;
using FlightBook.Services;
using Medallion.Threading;
using Medallion.Threading.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace FlightBook
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<FlightBookContext>(opt => opt.UseSqlServer(connectionString,
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly("FlightBook.Persistence.EFCore")));
            services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
            services.AddScoped<IFlightRegistrationCommands, FlightRegistrationCommands>();
            services.AddScoped<IFlightRegistrationService, FlightRegistrationService>();
            services.AddSingleton<IDistributedLockProvider>(_ => new SqlDistributedSynchronizationProvider(connectionString));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlightBook", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlightBook v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<FlightBookContext>();
                context.Database.Migrate();

                if (!context.Flights.Any())
                {
                    context.Flights.AddRange(DbSeed.Flights);

                    if (!context.Passengers.Any())
                    {
                        context.Passengers.AddRange(Enumerable.Range(1, 100).Select(x => new Passenger()));
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}