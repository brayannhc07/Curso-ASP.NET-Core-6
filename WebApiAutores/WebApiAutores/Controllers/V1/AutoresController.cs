using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1 {
	[ApiController]
	[Route( "api/autores" )]
	[CabeceraEstaPresente( "x-version", "1" )]
	//[Route("api/v1/autores")]
	[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin" )]
	//[ApiConventionType( typeof( DefaultApiConventions ) )]
	public class AutoresController: ControllerBase {
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;
		private readonly IAuthorizationService authorizationService;

		public AutoresController( ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService ) {
			this.context = context;
			this.mapper = mapper;
			this.authorizationService = authorizationService;
		}

		[HttpGet( Name = "obtenerAutoresv1" )] // api/autores
		[AllowAnonymous]
		[ServiceFilter( typeof( HATEOASAutorFilterAtrittribute ) )]
		public async Task<ActionResult<List<AutorDTO>>> Get( [FromQuery] PaginacionDTO paginacionDTO ) {
			var queryable = context.Autores.AsQueryable();
			await HttpContext.InsertarParametrosPaginacionEnCabecera( queryable );
			var autores = await queryable.OrderBy( autor => autor.Nombre ).Paginar( paginacionDTO ).ToListAsync();

			return mapper.Map<List<AutorDTO>>( autores );

		}


		[HttpGet( "{id:int}", Name = "obtenerAutorv1" )]
		[AllowAnonymous]
		[ServiceFilter( typeof( HATEOASAutorFilterAtrittribute ) )]
		public async Task<ActionResult<AutorDTOConLibros>> Get( int id ) {
			var autor = await context.Autores
				.Include( x => x.AutoresLibros )
					.ThenInclude( x => x.Libro )
				.FirstOrDefaultAsync( x => x.Id == id );

			if( autor is null ) {
				return NotFound();
			}

			var dto = mapper.Map<AutorDTOConLibros>( autor );
			return dto;
		}



		[HttpGet( "{nombre}", Name = "obtenerAutorPorNombrev1" )]
		public async Task<ActionResult<List<AutorDTOConLibros>>> GetPorNombre( [FromRoute] string nombre ) {
			var autores = await context.Autores.Where( x => x.Nombre.Contains( nombre ) ).ToListAsync();
			return mapper.Map<List<AutorDTOConLibros>>( autores );
		}

		[HttpPost( Name = "crearAutorv1" )]
		public async Task<ActionResult> Post( [FromBody] AutorCreacionDTO autorDTO ) {
			var existeAutorConElMismoNombre = await context.Autores
				.AnyAsync( x => x.Nombre == autorDTO.Nombre );

			if( existeAutorConElMismoNombre ) {
				return BadRequest( $"Ya existe un autor con el nombre {autorDTO.Nombre}" );
			}

			var autor = mapper.Map<Autor>( autorDTO );

			context.Add( autor );

			await context.SaveChangesAsync();

			var autorRespuesta = mapper.Map<AutorDTO>( autor );

			return CreatedAtRoute( "obtenerAutorv1", new { id = autor.Id }, autorRespuesta );
		}

		[HttpPut( "{id:int}", Name = "actualizarAutorv1" )]
		public async Task<ActionResult> Put( AutorCreacionDTO autorRequest, int id ) {
			var existe = await context.Autores.AnyAsync( x => x.Id == id );

			if( !existe )
				return NotFound();

			var autor = mapper.Map<Autor>( autorRequest );
			autor.Id = id;
			context.Update( autor );

			await context.SaveChangesAsync();

			return NoContent();
		}
		/// <summary>
		/// Borra un autor
		/// </summary>
		/// <param name="id">Id del autor a borrar</param>
		/// <returns></returns>

		[HttpDelete( "{id:int}", Name = "borrarAutorv1" )]
		public async Task<ActionResult> Delete( int id ) {

			var existe = await context.Autores.AnyAsync( x => x.Id == id );

			if( !existe )
				return NotFound();

			context.Remove( new Autor() { Id = id } );

			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
