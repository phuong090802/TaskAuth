public class NotFoundMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 404)
        {
            context.Response.ContentType = "text/html";

            string template = $@"
             <!DOCTYPE html>
             <html lang='en'>
             <head>
                 <meta charset='utf-8'>
                 <title>Error</title>
             </head>
             <body>
                 <pre>Cannot POST {context.Request.Path}</pre>
             </body>
             </html>";

            await context.Response.WriteAsync(template);
        }
    }
}