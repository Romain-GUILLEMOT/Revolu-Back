using WebApplication1.Database;

namespace WebApplication1.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RevoluDbContext dbContext)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var token = authorizationHeader.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Extraire le token après "Bearer "
                token = token.Substring("Bearer ".Length).Trim();

                var user = dbContext.Users.FirstOrDefault(u => u.Token == token);
                if (user != null)
                {
                    Console.WriteLine(token);

                    context.Items["User"] = user;
                }
            }
        }

        await _next(context);
    }
}