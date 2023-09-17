using Microsoft.AspNetCore.Authorization;

namespace TaskAuth.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class TokenCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var hasAuthorizeAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() != null;
            if (hasAuthorizeAttribute && !httpContext.User.Identity!.IsAuthenticated)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Message = "Không đủ quyền truy cập",
                    Code = -1000,
                    Success = false
                });
                return;
            }
            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class TokenCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenCheckMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenCheckMiddleware>();
        }
    }
}
