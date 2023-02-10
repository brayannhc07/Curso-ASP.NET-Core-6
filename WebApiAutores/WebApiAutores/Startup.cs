﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

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
					builder.WithOrigins( "https://apirequest.io" ).AllowAnyMethod().AllowAnyHeader();
				} );
			} );

			services.AddDataProtection();
			services.AddTransient<HashService>();
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

			app.UseCors();

			app.UseAuthorization();

			app.UseEndpoints( endpoints => {
				endpoints.MapControllers();
			} );
		}
	}
}
