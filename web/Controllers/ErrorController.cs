using Microsoft.AspNetCore.Mvc;

using web.Models;

namespace web.Controllers;
public class ErrorController : Controller
{
  public IActionResult NoAutorizado()
  {
    FullErrorViewModel vm = new()
    {
      Titulo = "Tu falta de permisos me resulta perturbadora",
      Mensaje = "No estas autorizado para acceder a esta parte de la aplicacion. Comunicate con el administrador!",
      Detalle = null,
      Source = null,
      Comunicacion = "Permisos insuficientes",
      TraceIdentifier = HttpContext.TraceIdentifier
    };

    return View("Errores/ErrorMejorado", vm);
  }
}