using System.Security.Claims;
using Entidades.Seguridad;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Servicios;
using Utiles;
using web.Models;
using web.Utiles;

namespace web.Controllers;

public class LoginController : Controller
{
  private readonly ISecurityServices _seguridad;
  private readonly ILogger<LoginController> _logger;

  public LoginController(ISecurityServices seguridad, ILogger<LoginController> logger)
  {
    _seguridad = seguridad;
    _logger = logger;
  }

  [HttpGet]
  public IActionResult Inicio()
  {
    //  muestra la pantalla de login para autenticar el usuario
    //
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Inicio(string login, string hashedPass)
  {
    //  validamos el modelo en el servidor
    //
    if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(hashedPass))
    {
      ModelState.AddModelError<object>(m => m, "Las credenciales estan vacias o incompletas");
      ModelState.AddModelError<object>(m => m, "Seria conveniente habilitar los scripts en el cliente!");

      return View();
    }

    try
    {
      Usuario userConectado = _seguridad.Login(login, hashedPass);

      //  Creamos una lista de Claims que describe nuestras caracteristicas 
      //  Algunos tipos de claims son standard mientras otros pueden ser customizados segun las
      //  necesidades de nuestra aplicacion
      //
      List<Claim> claims = new List<Claim>
      {
        new (ClaimTypes.Name, userConectado.Nombre),
        new (ClaimTypes.DateOfBirth, userConectado.Nacimiento.ToString("d")),
        new (ClaimTypes.NameIdentifier, userConectado.Clave.ToString())
        
      };

      foreach (var perfil in userConectado.Perfiles)
        claims.Add(new (ClaimTypes.Role, perfil.Nombre));

      //  Creamos una instancia de ClaimsIdentity que representara nuestra identidad principal
      //  (y unica por ahora) dentro de la instancia de ClaimsPrincipal
      //
      ClaimsIdentity identidad = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

      //  Por "casi" ultimo, generamos el ClaimsPrincipal que va a ser la informacion de autenticacion
      //  que se persistira durante la comunicacion HTTP en una cookie y se deserializa durante el
      //  procesamiento del pipeline (Authentication MW) y se coloca en la propiedad User
      //
      ClaimsPrincipal userPrincipal = new(identidad);

      //  Y por ultimo en serio, ejecutamos el metodo SignIn() de HttpContext (un metodo de extension)
      //  que en realidad llama a dicho metodo de CookieAuthenticationHandler (el que hace el trabajo
      //  real en el esquema!!)
      //  Si tuvieramos que cambiar algo de la cookie (por ejemplo su caducidad) necesitariamos
      //  ademas una instancia de AuthenticationProperties para pasar en el momento del sign-in
      //
      AuthenticationProperties properties = new AuthenticationProperties
      {
        //IsPersistent = false
      };
      HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, properties);

      //  lo hacemos critico para que se distinga en la pantalla de log
      //
      if (userConectado != null)
        _logger.LogCritical("El usuario {user} se conecto correctamente", userConectado.Clave);

      //  HttpContext.Session.Set("USUARIO", userConectado);

      //  tenemos que usar this porque un metodo de extension necesita una instancia para ser
      //  invocado, recordar que es un metodo static!!
      //  La solucion es crear un ControllerMejorado que derive de Controller y agregar los
      //  metodos de sesion ahi
      //
      this.SetSession("USUARIO", userConectado);

      return RedirectToAction("Index", "Home");
    }
    catch (Exception ex)
    {
      //  puede ser usuario y/o pass
      //
      _logger.LogCritical(ex,
        "Cuidado muchas repeticiones de esta excepcion puede significar que nos quieren atacar");

      FullErrorViewModel vm = new()
      {
        Titulo = "Parece que la fuerza no te acompaña...",
        Mensaje = "Las credenciales no son validas",
        Detalle = ex.Resumen(),
        Source = "Login Usuario",
        Comunicacion = "Que pasa que no puedo entrar??",
        TraceIdentifier = HttpContext.TraceIdentifier
      };
      return View("Errores/ErrorMejorado", vm);
    }
  }

  [HttpGet]
  public IActionResult Logout()
  {
    //  por ahora solo eliminamos el usuario en la sesion
    //
    this.SetSession<Usuario>("USUARIO", null);
    
    HttpContext.SignOutAsync();

    return RedirectToAction("Index", "Home");
  }

  [HttpGet]
  public IActionResult Restart()
  {
    //  eliminamos todas las variables de la sesion
    //
    this.HttpContext.Session.Clear();

    HttpContext.SignOutAsync();

    return RedirectToAction("Inicio", "Login");
  }
}
