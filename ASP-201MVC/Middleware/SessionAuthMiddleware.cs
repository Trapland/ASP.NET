using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using System.Security.Claims;

namespace ASP_201MVC.Middleware
{
    public class SessionAuthMiddleware 
    {
        // Для утворення ланцюга кожна ланка(об'єкт Middleware) отримує
        // посилання на наступну ланку. Це посилання передається через конструктор
        // і має бути збережене у об'єкті

        private readonly RequestDelegate _next;

        // Middleware існує протягом всієї роботи програми (Singleton), тому
        // інжекція сервісів через конструктор не здійснюється

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        // Обов'язковий для Middleware метод InvokeAsync або Invoke(старий підхід)
        // Перший параметр - завжди HttpContext context,
        // за потреби, наступні параметри - інжекційні

        public async Task InvokeAsync(HttpContext context, ILogger<SessionAuthMiddleware> logger, DataContext dataContext)
        {
            //logger.LogInformation("SessionAuthMiddleware works");
            // Перевіряємо наявність у сесії authUserId (встановлюється при
            // автентифікації у UserController::AuthUser() )
            String? userId = context.Session.GetString("authUserId");
            if(userId is not null)
            {
                try
                {
                    User? authUser = dataContext.Users.Find(Guid.Parse(userId));
                    if(authUser is not null)
                    {
                        context.Items.Add("authUser", authUser);
                        Claim[] claims = new Claim[]
                        {
                            new Claim(ClaimTypes.Sid, userId),
                            new Claim(ClaimTypes.Name, authUser.Name),
                            new Claim(ClaimTypes.NameIdentifier, authUser.Login),
                            new Claim(ClaimTypes.UserData, authUser.Avatar ?? String.Empty)
                        };
                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(claims,
                            nameof(SessionAuthMiddleware)));
                        context.User = principal;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "SessionAuthMiddleware");
                }
            }
            // для продовження ланцюга метод має викликати наступну
            // ланку Middleware
            await _next(context);
        }
    }

    // Включення класу SessionAuthMiddleware у ланцюг Middleware здійснюється
    // У Program.cs командою
    // app.UseMiddleware<SessionAuthMiddleware>();
    // Традиційно Middleware забезпечують розширеннями (extensions) для
    // вживання UseXxxxx формалізму (Ххххх - назва Middleware)
    // app.UseSessionAuth();

    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
