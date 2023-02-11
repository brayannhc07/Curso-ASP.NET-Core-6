using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApiAutores.Utilidades {
	public class AgregarParametroHATEOAS: IOperationFilter {
		public void Apply( OpenApiOperation operation, OperationFilterContext context ) {

			if( context.ApiDescription.HttpMethod != "GET" ) {
				return;
			}

			operation.Parameters ??= new List<OpenApiParameter>();

			operation.Parameters.Add( new OpenApiParameter {
				Name = "incluirHATEOAS",
				In = ParameterLocation.Header,
				Required = false,
			} );
		}
	}
}
