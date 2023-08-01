using System.Text.Json;
using Entities.DTOs;
using Services;

namespace ProductManagement.Helpers
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        /// <summary>
        /// This function catches different types of exceptions that may occur during HTTP requests globally.
        /// </summary>
        /// <param name="HttpContext">Represents the context in which the HTTP request is being processed,
        /// including information about the request and response.</param>
        /// <param name="RequestDelegate">RequestDelegate is a delegate that represents the next middleware
        /// component in the pipeline. It is responsible for invoking the next middleware component in the
        /// pipeline.</param>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {            
            try
            {
                await next(context);
            }

            catch (BadRequestException b)
            {
                await context.Response.WriteAsync(HandleException(400, "Bad Request", b.Message, context));
            }

            catch (NotFoundException n)
            {
                await context.Response.WriteAsync(HandleException(404, "Not found", n.Message, context));
            }

            catch (ConflictException c)
            {
                await context.Response.WriteAsync(HandleException(409, "Conflict", c.Message, context));
            }

            catch (Exception ex)
            {
                await context.Response.WriteAsync(HandleException(500, "An error occurred while processing your request", ex.Message, context));
            }
        }

        /// <summary>
        /// This function handles exceptions by creating an error response object, serializing it to JSON,
        /// setting the response content type and status code, and returning the JSON string.
        /// </summary>
        /// <param name="statusCode">an integer representing the HTTP status code to be returned in the
        /// response.</param>
        /// <param name="message">A brief message describing the error that occurred.</param>
        /// <param name="description">A string that provides additional information or context about the error
        /// that occurred. It can be used to give more details about the error message.</param>
        /// <param name="HttpContext">HttpContext is an object that encapsulates all HTTP-specific information
        /// about an individual HTTP request. It contains information about the request, such as the URL,
        /// headers, and body, as well as the response, such as the status code, headers, and body. It also
        /// provides access to the server's configuration</param>
        /// <returns>
        /// The method is returning a string that represents a JSON object containing an error response with a
        /// status code, message, and description.
        /// </returns>
        public string HandleException(int statusCode, string message, string description, HttpContext context)
        {
            ErrorResponseDto errorResponse = new ErrorResponseDto() { StatusCode = statusCode, Message = message, Description = description };

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            };

            string json = JsonSerializer.Serialize(errorResponse, options);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return json;
        }
    }
}