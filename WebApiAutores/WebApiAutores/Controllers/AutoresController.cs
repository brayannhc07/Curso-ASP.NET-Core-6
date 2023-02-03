using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
	[Route( "api/autores" )]
	public class AutoresController: ControllerBase {
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public AutoresController( ApplicationDbContext context, IMapper mapper ) {
			this.context = context;
			this.mapper = mapper;
		}

		[HttpGet] // api/autores
		public async Task<ActionResult<List<AutorDTO>>> Get() {
			var autores = await context.Autores.ToListAsync();

			return mapper.Map<List<AutorDTO>>( autores );
		}


		[HttpGet( "{id:int}", Name = "obtenerAutor" )]
		public async Task<ActionResult<AutorDTOConLibros>> Get( int id ) {
			var autor = await context.Autores
				.Include( x => x.AutoresLibros )
					.ThenInclude( x => x.Libro )
				.FirstOrDefaultAsync( x => x.Id == id );

			if( autor is null ) {
				return NotFound();
			}

			return mapper.Map<AutorDTOConLibros>( autor );
		}

		[HttpGet( "{nombre}" )]
		public async Task<ActionResult<List<AutorDTOConLibros>>> Get( [FromRoute] string nombre ) {
			var autores = await context.Autores.Where( x => x.Nombre.Contains( nombre ) ).ToListAsync();
			return mapper.Map<List<AutorDTOConLibros>>( autores );
		}

		[HttpPost]
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

			return CreatedAtRoute( "obtenerAutor", new { id = autor.Id }, autorRespuesta );
		}

		[HttpPut( "{id:int}" )]
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

		[HttpDelete( "{id:int}" )]
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
