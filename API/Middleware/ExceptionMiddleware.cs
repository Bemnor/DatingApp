using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env
    )
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    //exception catching middleware
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            //log to server console
            _logger.LogError(ex, ex.Message);
            //make exception response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            //if in development, show StackTrace, if not.. just send "internal server error"
            var response = _env.IsDevelopment()
                // the '?' in 'ex.StackTrace?' tells the compiler it can be null as to not throw an error.
                ? new ApiException(
                    context.Response.StatusCode,
                    ex.Message,
                    ex.StackTrace?.ToString()
                )
                : new ApiException(
                    context.Response.StatusCode,
                    ex.Message,
                    "Internal Server Error"
                );

            //some formatting for the response
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            //update response
            await context.Response.WriteAsync(json);
        }
    }
}
