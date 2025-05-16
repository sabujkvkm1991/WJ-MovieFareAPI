using System.Net;
using System.Text.Json;

namespace MovieFare.Middlewares
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;
		private readonly IHostEnvironment _hostEnvironment;

		public ExceptionHandlingMiddleware(RequestDelegate next, 
			ILogger<ExceptionHandlingMiddleware> logger,
			IHostEnvironment hostEnvironment)
		{
			_next = next;
			_logger = logger;
			_hostEnvironment = hostEnvironment;
		}

		/// <summary>
		/// Middleware to handle exceptions globally.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context); 
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception occurred");

				context.Response.ContentType = "application/json";
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				
				var errorResponse = new
				{
					StatusCode = context.Response.StatusCode,
					Message = "An unexpected error occurred. Please try again later.",
					Details = _hostEnvironment.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later."
				};

				var json = JsonSerializer.Serialize(errorResponse);
				await context.Response.WriteAsync(json);
			}
		}		
	}
}
