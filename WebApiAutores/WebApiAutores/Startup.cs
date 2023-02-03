using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;

namespace WebApiAutores {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public void ConfigureServices( IServiceCollection services ) {

			// Add services to the container.

			services.AddControllers(options => {
				options.Filters.Add( typeof( FiltroDeExcepcion ) );
			} ).AddJsonOptions( x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
			.AddNewtonsoftJson();

			services.AddDbContext<ApplicationDbContext>( options => 
				options.UseSqlServer( 
					Configuration.GetConnectionString( "DefaultConnection" ) ) );

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();

			services.AddAutoMapper( typeof( Startup ) );
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger ) {
			// Configure the HTTP request pipeline.
			app.UseLoguearRespuestaHTTP();

			if( env.IsDevelopment() ) {
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			} );
		}
	}
}
