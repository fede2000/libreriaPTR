using Entidades;
using Entidades.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Servicios;
using Utiles;
using web.Models;

namespace web.Controllers;

public class UsuarioController : Controller
{
	private readonly ISecurityServices _security;
	private readonly ILogger<EmpleadoController> _logger;

	public UsuarioController(ISecurityServices security, ILogger<EmpleadoController> logger)
	{
		_security = security;
		_logger = logger;
	}

	[HttpGet]
	public IActionResult Nuevo()
	{
		var vm = new NewUsuarioViewModel
		{
			Usuario = new Usuario(),
			PerfilesDisponibles = _security.GetPerfiles()
								  .Where(p => p.TipoUsuario == TipoUsuario.Empleado)
								  .Select(p => new SelectListItem(p.Nombre, p.ID.ToString())),
			PerfilesAsignados = null




		}

		;
		return View(vm);
	}

	[HttpPost]
	public IActionResult Nuevo([Bind(Prefix = "Usuario")] Usuario user, string hashedPass, int[] perfilesAsignados)
	{
		//  si el modelo es invalido al comienzo significa que algo no funciono bien en el proceso de
		//  binding con el modelo
		//  Deberiamos revisar cada uno de los items de ModelState que genera el model binding para analizar
		//  cual de ellos contiene el error
		//
		if (!ModelState.IsValid)
		{
			var errores = ModelState
							.Where(ms => ms.Value.ValidationState == ModelValidationState.Invalid)
							.Select(par => par.Value)
							.ToList();

			//  como sabemos que el problema esta en la fecha de nacimiento, modificamos esa entrada para reemplazar
			//  el mensaje de error por defecto por uno propio. El error original debe borrarse ya que el tag-helper
			//  solo muestra un mensaje de error en pantalla
			//
			var errNacimiento = ModelState["Usuario.Nacimiento"];
			errNacimiento.Errors.Clear();
			errNacimiento.Errors.Add("La fecha de nacimiento tiene un formato invalido");

			//
			return ReenviarFormulario();
		}
		//  Si el ModelState es valido, implica que se pudo hacer el binding correctamente
		//  Aun asi, tenemos que repetir cada validacion que hicimos en el cliente...
		//  como ejemplo solo haremos una
		//  Ademas podemos incluir reglas de negocio por ejemplo que la fecha de nacimiento sea posterior a 1900
		//
		if (string.IsNullOrWhiteSpace(user.Login))
		{
			ModelState.AddModelError<NewUsuarioViewModel>(vm => vm.Usuario.Login, "El login no puede estar vacio");
		}
		if (user.Nacimiento.Year <= 1900)
		{
			ModelState.AddModelError<NewUsuarioViewModel>(vm => vm.Usuario.Nacimiento,
			  "El año de nacimiento deberia ser superior a 1900");
		}
		//
		//  Luego de todas las validaciones del servidor, chequeamos nuevamente si el model state es invalido
		//  Cada vez que usamos AddModelError la cantidad de errores del modelo se incremente y ademas el estado
		//  queda en Invalid (para que podamos reenviar la pagina de edicion)
		//
		if (!ModelState.IsValid)
			return ReenviarFormulario();

		try
		{
			var nuevoUsuario = _security.CrearCliente(user.Nombre, user.Correo, user.Login,
			  hashedPass, user.Nacimiento);

			//  notificacion con el nuevo ID del usuario...
			_logger.LogInformation("Nuevo usuario ingresado con clave = {ID}", nuevoUsuario.Clave);

			return RedirectToAction("Index", "Home");
		}
		catch (Exception ex)
		{
			FullErrorViewModel err =
			  new()
			  {
				  Titulo = "Houston...tenemos un problema",
				  Mensaje = "Se produjo un error intentando guardar el nuevo usuario",
				  Detalle = ex.Resumen(),
				  TraceIdentifier = HttpContext.TraceIdentifier,
				  Comunicacion = "Por favor comunicarse con soporte tecnico",
				  Source = "Nuevo Usuario"
			  };

			return View("Errores/ErrorMejorado", err);
		}

		#region FUNCIONES LOCALES

		IActionResult ReenviarFormulario()
		{
			//  ToList() me obliga a dejarlos en memoria
			//
			var allPerfiles = _security
			  .GetPerfiles()
			  .Where(p => p.TipoUsuario == TipoUsuario.Cliente)
			  .ToList();

			var vm = new NewUsuarioViewModel
			{
				Usuario = user,
				PerfilesDisponibles = allPerfiles.ExceptBy(perfilesAsignados, p => p.ID)
				.Select(p => new SelectListItem(p.Nombre, p.ID.ToString())),
				PerfilesAsignados = allPerfiles.IntersectBy(perfilesAsignados, p => p.ID)
				.Select(p => new SelectListItem(p.Nombre, p.ID.ToString()))
			};
			return View(vm);
		}

		#endregion
	}
}

public class NewUsuarioViewModel
{
	public Usuario Usuario { get; set; }

	/// <summary>
	/// Lista de <see cref="SelectListItem"/> que representan perfiles que podemos todavia asignar al usuario
	/// </summary>
	public IEnumerable<SelectListItem> PerfilesDisponibles { get; set; }

	/// <summary>
	/// Lista de <see cref="SelectListItem"/> que representan los perfiles ya asignados al usuario si estoy editando
	/// o una lista vacia si estoy creando uno nuevo
	/// </summary>
	public IEnumerable<SelectListItem> PerfilesAsignados { get; set; }
}