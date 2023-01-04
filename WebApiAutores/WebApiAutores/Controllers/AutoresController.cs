using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers {
	[ApiController]
	[Route( "api/autores" )]
	public class AutoresController: ControllerBase {
		private readonly ApplicationDbContext context;
		private readonly IServicio servicio;
		private readonly ServicioTransient servicioTransient;
		private readonly ServicioScoped servicioScoped;
		private readonly ServicioSingleton servicioSingleton;
		private readonly ILogger<AutoresController> logger;

		public AutoresController( ApplicationDbContext context, IServicio servicio,
			ServicioTransient servicioTransient, ServicioScoped servicioScoped,
			ServicioSingleton servicioSingleton, ILogger<AutoresController> logger ) {
			this.context = context;
			this.servicio = servicio;
			this.servicioTransient = servicioTransient;
			this.servicioScoped = servicioScoped;
			this.servicioSingleton = servicioSingleton;
			this.logger = logger;
		}

		[HttpGet( "GUID" )]
		//[ResponseCache( Duration = 10 )]
		[ServiceFilter(typeof(MiFiltroDeAccion))]
		public ActionResult ObtenerGuids() {
			return Ok( new {
				Transient = servicioTransient.Guid,
				ServicioTransient = servicio.ObtenerTransient,
				Scoped = servicioScoped.Guid,
				ServicioScoped = servicio.ObtenerScoped,
				Singleton = servicioSingleton.Guid,
				ServicioSingleton = servicio.ObtenerSingleton
			} );
		}

		[HttpGet] // api/autores
		[HttpGet( "listado" )] // api/autores/listado
		[HttpGet( "/listado" )] // listado
		//[Authorize]
		[ServiceFilter( typeof( MiFiltroDeAccion ) )]
		public async Task<ActionResult<List<Autor>>> Get() {
			throw new NotImplementedException();
			logger.LogInformation( "Estamos obteniendo los autores" );
			logger.LogWarning( "Mensaje de prueba" );
			return await context.Autores.Include( x => x.Libros ).ToListAsync();
		}

		[HttpGet( "primero" )] // api/autores/primero?nombre=bryan&apellido=hernandez
		public async Task<ActionResult<Autor>> PrimerAutor( [FromHeader] int miValor, [FromQuery] string miNombre ) {
			return await context.Autores.FirstOrDefaultAsync();
		}

		[HttpGet( "{id:int}" )]
		public async Task<ActionResult<Autor>> Get( int id ) {
			var autor = await context.Autores.FirstOrDefaultAsync( x => x.Id == id );

			if( autor is null ) {
				return NotFound();
			}

			return autor;
		}

		[HttpGet( "{nombre}" )]
		public async Task<ActionResult<Autor>> Get( [FromRoute] string nombre ) {
			var autor = await context.Autores.FirstOrDefaultAsync( x => x.Nombre.Contains( nombre ) );

			if( autor is null ) {
				return NotFound();
			}

			return autor;
		}

		[HttpPost]
		public async Task<ActionResult> Post( [FromBody] Autor autor ) {
			var existeAutorConElMismoNombre = await context.Autores
				.AnyAsync( x => x.Nombre == autor.Nombre );

			if( existeAutorConElMismoNombre ) {
				return BadRequest( $"Ya existe un autor con el nombre {autor.Nombre}" );
			}

			context.Add( autor );

			await context.SaveChangesAsync();

			return Ok();
		}

		[HttpPut( "{id:int}" )]
		public async Task<ActionResult> Put( Autor autor, int id ) {
			if( autor.Id != id ) {
				return BadRequest( "El id del autor no coindcide con el id de la url" );
			}

			var existe = await context.Autores.AnyAsync( x => x.Id == id );

			if( !existe )
				return NotFound();

			context.Update( autor );

			await context.SaveChangesAsync();

			return Ok();
		}

		[HttpDelete( "{id:int}" )]
		public async Task<ActionResult> Delete( int id ) {

			var existe = await context.Autores.AnyAsync( x => x.Id == id );

			if( !existe )
				return NotFound();

			context.Remove( new Autor() { Id = id } );

			await context.SaveChangesAsync();

			return Ok();
		}
	}
}
