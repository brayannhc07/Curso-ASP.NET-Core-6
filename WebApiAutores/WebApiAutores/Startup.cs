﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

[assembly: ApiConventionType( typeof( DefaultApiConventions ) )]
namespace WebApiAutores {
	public class Startup {
		public Startup( IConfiguration configuration ) {
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public void ConfigureServices( IServiceCollection services ) {

			// Add services to the container.

			services.AddControllers( options => {
				options.Filters.Add( typeof( FiltroDeExcepcion ) );
				options.Conventions.Add( new SwaggerAgrupaPorVersion() );
			} ).AddJsonOptions( x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles )
			.AddNewtonsoftJson();

			services.AddDbContext<ApplicationDbContext>( options =>
				options.UseSqlServer(
					Configuration.GetConnectionString( "DefaultConnection" ) ) );

			services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
				.AddJwtBearer( opciones => opciones.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes( Configuration["llavejwt"] ) ),
					ClockSkew = TimeSpan.Zero
				} );

			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen( c => {
				c.SwaggerDoc( "v1", new OpenApiInfo {
					Title = "WebApiAutores",
					Version = "v1",
					Description = "Este es un webapi para trabajar con libros y autores",
					Contact = new OpenApiContact {
						Email = "brayannhc07@gmail.com",
						Name = "Bryan Hernández",
						Url = new Uri( "https://bryan.blog" )
					},
					License = new OpenApiLicense {
						Name = "MIT"
					}
				} );
				c.SwaggerDoc( "v2", new OpenApiInfo { Title = "WebApiAutores", Version = "v2" } );

				c.OperationFilter<AgregarParametroHATEOAS>();
				c.OperationFilter<AgregarParametroXVersion>();
				c.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme {
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					BearerFormat = "JWT",
					In = ParameterLocation.Header
				} );

				c.AddSecurityRequirement( new OpenApiSecurityRequirement {
					{
						new OpenApiSecurityScheme {
							Reference = new OpenApiReference {
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				} );

				var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var rutaXML = Path.Combine( AppContext.BaseDirectory, archivoXML );
				c.IncludeXmlComments( rutaXML );
			} );

			services.AddAutoMapper( typeof( Startup ) );

			services.AddIdentity<IdentityUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddAuthorization( opciones => {
				opciones.AddPolicy( "EsAdmin", politica => politica.RequireClaim( "EsAdmin" ) );
			} );

			services.AddCors( opciones => {
				opciones.AddDefaultPolicy( builder => {
					builder.WithOrigins( "https://apirequest.io" ).AllowAnyMethod().AllowAnyHeader()
					.WithExposedHeaders( new string[] { "cantidadTotalRegistros" } );
				} );
			} );

			services.AddDataProtection();
			services.AddTransient<HashService>();

			services.AddTransient<GeneradorEnlaces>();
			services.AddTransient<HATEOASAutorFilterAtrittribute>();
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger ) {
			// Configure the HTTP request pipeline.
			app.UseLoguearRespuestaHTTP();

			if( env.IsDevelopment() ) {
				app.UseSwagger();
				app.UseSwaggerUI( c => {
					c.SwaggerEndpoint( "/swagger/v1/swagger.json", "WebApiAutores v1" );
					c.SwaggerEndpoint( "/swagger/v2/swagger.json", "WebApiAutores v2" );
				} );
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors();

			app.UseAuthorization();

			app.UseEndpoints( endpoints => {
				endpoints.MapControllers();
			} );
		}
	}
}
