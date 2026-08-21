using System.Net;
using System.Text.Json;
using Domain.Exceptions;

namespace API.Middlewares;

public class ExceptionHandlingMiddleware
{
      private readonly RequestDelegate _next;
      private readonly ILogger<ExceptionHandlingMiddleware> _logger;

      public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
      {
            _next = next;
            _logger = logger;
      }

      public async Task InvokeAsync(HttpContext context)
      {
            try
            {
                  await _next(context);
            }
            catch (Exception ex)
            {
                  await HandleExceptionAsync(context, ex);
            }
      }

      private async Task HandleExceptionAsync(HttpContext context, Exception exception)
      {
            context.Response.ContentType = "application/json";

            var (statusCode, title) = exception switch
            {
                  NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                  UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                  ConflictException => (HttpStatusCode.Conflict, "Conflict"),
                  ValidationException => (HttpStatusCode.BadRequest, "Validation error"),
                  BadRequestException => (HttpStatusCode.BadRequest, "Bad request"),
                  _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                  _logger.LogError(exception, "Unhandled exception occurred");
            else
                  _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                  title,
                  status = (int)statusCode,
                  errors = exception is ValidationException validationEx ? validationEx.Errors : null,
                  detail = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred." : exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
      }
}
