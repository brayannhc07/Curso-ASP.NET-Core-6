
namespace WebApiAutores.Middlewares {

	public static class LoguearRespuestaHTTPMiddlewareExtensions {
		public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app) {
			return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
		}
	}

	public class LoguearRespuestaHTTPMiddleware {
		private readonly RequestDelegate next;
		private readonly ILogger<LoguearRespuestaHTTPMiddleware> logger;

		public LoguearRespuestaHTTPMiddleware(RequestDelegate next,
			ILogger<LoguearRespuestaHTTPMiddleware> logger ) {
			this.next = next;
			this.logger = logger;
		}

		// Invokeo InvokeAsync
		public async Task InvokeAsync(HttpContext context) {
			using var memoryStream = new MemoryStream();

			var cuerpoOriginalRespuesta = context.Response.Body;

			context.Response.Body = memoryStream;
			await next(context);

			memoryStream.Seek( 0, SeekOrigin.Begin );
			string respuesta = new StreamReader( memoryStream ).ReadToEnd();
			memoryStream.Seek( 0, SeekOrigin.Begin );

			await memoryStream.CopyToAsync( cuerpoOriginalRespuesta );
			context.Response.Body = cuerpoOriginalRespuesta;

			logger.LogInformation( respuesta );
		}
	}
}
