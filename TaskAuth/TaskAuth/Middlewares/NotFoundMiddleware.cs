namespace TaskAuth.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        public NotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);
            if (httpContext.Response.StatusCode == 405)
            {
                httpContext.Response.ContentType = "text/html";

                string template = $@"
                 <!DOCTYPE html>
                 <html lang='en'>
                 <head>
                     <meta charset='utf-8'>
                     <title>Error</title>
                 </head>
                 <body>
                     <pre>Cannot POST {httpContext.Request.Path}</pre>
                 </body>
                 </html>";
                await httpContext.Response.WriteAsync(template);
            }

        }

    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NotFoundMiddlewareExtensions
    {
        public static IApplicationBuilder UseNotFoundMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NotFoundMiddleware>();
        }
    }
}
