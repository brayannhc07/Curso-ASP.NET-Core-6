namespace WebApiAutores.DTOs {
	public class PaginacionDTO {
		public int Pagina { get; set; } = 1;
		private int recordsPorPagina = 10;
		private readonly int cantidadMaximaPorPagina = 50;

		public int RecordPorPagina {
			get => recordsPorPagina;
			set => recordsPorPagina = ( value > cantidadMaximaPorPagina ) ? cantidadMaximaPorPagina : value;
		}
	}
}
