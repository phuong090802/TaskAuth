using Microsoft.AspNetCore.Authorization;

namespace TaskAuth.Middlewares
{
    public class TokenCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var hasAuthorizeAttribute = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() != null;
            if (hasAuthorizeAttribute && !context.User.Identity!.IsAuthenticated)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "Không đủ quyền truy cập",
                    Code = -1000,
                    Success = false
                });

                return;
            }
            await _next(context);
        }
    }
}
