using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers {
	[ApiController]
	[Route( "api/libros" )]
	public class LibrosController: ControllerBase {
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public LibrosController( ApplicationDbContext context, IMapper mapper ) {
			this.context = context;
			this.mapper = mapper;
		}

		[HttpGet( "{id:int}", Name = "obtenerLibro" )]
		public async Task<ActionResult<LibroDTOConAutores>> Get( int id ) {
			var libro = await context.Libros
				.Include( x => x.AutoresLibros.OrderBy( y => y.Order ) )
					.ThenInclude( x => x.Autor )
				.Include( x => x.Comentarios )
				.FirstOrDefaultAsync( x => x.Id == id );
			return mapper.Map<LibroDTOConAutores>( libro );
		}

		[HttpPost]
		public async Task<ActionResult> Post( LibroCreacionDTO libroDTO ) {

			if( libroDTO.AutoresIds is null ) {
				return BadRequest( "No se puede crear un libro sin autores." );
			}

			var autoresIds = await context.Autores
				.Where( x => libroDTO.AutoresIds.Contains( x.Id ) )
				.Select( x => x.Id ).ToListAsync();

			if( libroDTO.AutoresIds.Count != autoresIds.Count ) {
				return BadRequest( "No existe alguno de los autores enviados." );
			}
			var libro = mapper.Map<Libro>( libroDTO );

			AsignarOrdenAutores( libro );

			context.Add( libro );
			await context.SaveChangesAsync();

			var libroRespuesta = mapper.Map<LibroDTO>( libro );

			return CreatedAtRoute( "obtenerLibro", new { id = libro.Id }, libroRespuesta );
		}

		[HttpPut( "{id:int}" )]
		public async Task<ActionResult> Put( int id, LibroCreacionDTO libroRequest ) {
			var libroDB = await context.Libros
				.Include( x => x.AutoresLibros )
				.FirstOrDefaultAsync( x => x.Id == id );

			if( libroDB is null ) {
				return NotFound();
			}

			libroDB = mapper.Map( libroRequest, libroDB );
			AsignarOrdenAutores( libroDB );

			await context.SaveChangesAsync();

			return NoContent();

		}

		[HttpPatch( "{id:int}" )]
		public async Task<ActionResult> Patch( int id, JsonPatchDocument<LibroPathDTO> patchDocument ) {
			if( patchDocument is null ) {
				return BadRequest();
			}

			var libroDB = await context.Libros.FirstOrDefaultAsync( x => x.Id == id );

			if( libroDB is null ) {
				return NotFound();
			}

			var libroDTO = mapper.Map<LibroPathDTO>( libroDB );

			patchDocument.ApplyTo( libroDTO, ModelState );

			var esValido = TryValidateModel( libroDTO );

			if( !esValido ) {
				return BadRequest( ModelState );
			}

			mapper.Map( libroDTO, libroDB );

			await context.SaveChangesAsync();

			return NoContent();
		}

		private static void AsignarOrdenAutores( Libro libro ) {
			if( libro.AutoresLibros is not null ) {
				for( int i = 0; i < libro.AutoresLibros.Count; i++ ) {
					libro.AutoresLibros[i].Order = i;
				}
			}
		}


		[HttpDelete( "{id:int}" )]
		public async Task<ActionResult> Delete( int id ) {

			var existe = await context.Libros.AnyAsync( x => x.Id == id );

			if( !existe )
				return NotFound();

			context.Remove( new Libro() { Id = id } );

			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
