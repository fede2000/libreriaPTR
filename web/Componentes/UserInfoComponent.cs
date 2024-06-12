using Entidades;
using Entidades.Seguridad;
using Microsoft.AspNetCore.Mvc;
using web.Utiles;

namespace web.Componentes;

public class UserInfoComponent : ViewComponent
{
  public IViewComponentResult Invoke()
  {
    if (HttpContext.Session.IsAvailable)
    {
      var user = HttpContext.Session.Get<Usuario>("USUARIO");

      if (user != null)
      {
        if (user.TipoUsuario == TipoUsuario.Cliente)
          return View("Usuario", user);
        else
        {
          return View("Empleado", user);
        }

      }

      return View("UserGenerico");
    }
    //  si el user no esta logueado retornar un fragmento generico
    return View("UserGenerico");
  }
}
