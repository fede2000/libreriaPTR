using Microsoft.AspNetCore.Authentication;

namespace web.Middleware;

/// <summary>
/// Middleware para deslogeo automatico del usuario persistido en la cookie de autenticacion
/// Chrome (y tal vez otros navegadores) no tratan las cookies de sesion como tales, aparentemente
/// por la existencia de la configuracion "session restore"
/// Por otro lado la cookie de sesion ASPNET expira a los 20 minutos (default) o bien cuando el
/// servidor se reinicia
/// Para abarcar mas casos deberiamos tener una ventana de 10 minutos por ejemplo
/// Tambien es cierto que lo importante es que el usuario de sesion sea el autenticado
/// Cuando el server se reinicia
/// La cookie de sesion no es valida porque la session key ya no existe
/// La cookie de autenticacion es valida porque el contenido esta dentro de la misma cookie
/// </summary>
public class AutoSignOutMiddleware
{
  private readonly RequestDelegate _next;

  public AutoSignOutMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    if (!context.Session.TryGetValue("USUARIO", out _))
    {
      if (context.User?.Identity?.IsAuthenticated ?? false)
      {
        await context.SignOutAsync();
      }
    }
    //  si la sesion esta disponible...seguimos...
    await _next(context);
  }
}
