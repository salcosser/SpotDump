using Microsoft.OpenApi.Models;
using SpotDump.Models;
using SpotDump.WebApi.Repositories.DI;
namespace SpotDump.WebApi {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.Configure<SpotifyAuthOptions>(builder.Configuration.GetSection(SpotifyAuthOptions.ConfigurationSectionName));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SpotDump API",
                    Version = "v1",
                    Description = "Simple Spotify wrapper with playlist export capabilities."
                });
            });

            builder.Services.AddRepositoryServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
