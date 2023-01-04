using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

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
			} ).AddJsonOptions( x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

			services.AddDbContext<ApplicationDbContext>( options => options.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ) ) );

			// Siempre una instancia nueva (tareas sin estado)
			//services.AddTransient<IServicio, ServicioA>(); 
			//services.AddTransient<ServicioA>();

			// Una instancia nueva por HTTP Context (sesión o usuario)
			//services.AddScoped<IServicio, ServicioB>();

			// Siempre una sola instancia (para datos en memoria, como caché)
			services.AddTransient<IServicio, ServicioA>();

			services.AddTransient<ServicioTransient>();
			services.AddScoped<ServicioScoped>();
			services.AddSingleton<ServicioSingleton>();
			services.AddTransient<MiFiltroDeAccion>();
			services.AddHostedService<EscribirEnArchivo>();

			services.AddResponseCaching();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger ) {
			// Configure the HTTP request pipeline.
			app.UseLoguearRespuestaHTTP();

			app.Map( "/ruta1", app => {
				app.Run( async contexto => {
					await contexto.Response.WriteAsync( "Estoy interceptando la tubería" );
				} );
			} );

			if( env.IsDevelopment() ) {
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseResponseCaching();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			} );
		}
	}
}
