using Datos.Repositorios;
using Entidades;
using Entidades.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Servicios;
using System.Security.Claims;
using Utiles;
using web.Models;

namespace web.Controllers;

[Authorize(Roles = "Visitante")]
public class DireccionController : Controller
{
    private readonly ISecurityServices _security;
    private readonly ILogger<EmpleadoController> _logger;

    public DireccionController(ISecurityServices security, ILogger<EmpleadoController> logger)
    {
        _security = security;
        _logger = logger;
    }

    [HttpGet]
    // Accion a la vista parcial AgregarDireccion
    public IActionResult AgregarDireccion()
    {
       
        return View(new Direccion());
    }

    // Acción para manejar el envío del formulario de creación de dirección
    [HttpPost]
    public async Task<IActionResult> AgregarDireccion(string linea1, string linea2, string codigoPostal, string provincia, string localidad, string pais)
    {
        try
        {
            // Obtener el ID del usuario logueado (modiique el claimtypes.NameIdentifier para que se almacene la clave GUID del usuario logueado)
            var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            // Verificar si existe un usuario logueado
            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized(); 
            }

            // se envian los datos de la nueva direccion a ISecurityServices.crearDireccionAsync
            var direccion = await _security.CrearDireccionAsync(linea1, linea2, codigoPostal, provincia, localidad, pais, Guid.Parse(idUsuario));
            _logger.LogInformation("Dirección creada exitosamente.");
            //return RedirectToAction("Detalles", new { id = direccion.ID });
            return RedirectToAction("Direcciones");
        }
        catch (Exception ex)
        {
            
            //
            _logger.LogCritical(ex,
              "Cuidado muchas repeticiones de esta excepcion puede significar que nos quieren atacar");

            FullErrorViewModel vm = new()
            {
                Titulo = "Parece que la fuerza no te acompaña...",
                Mensaje = "Hubo un problema al agregar la dirección",
                Detalle = ex.Resumen(),
                Source = "Agregar Dirección",
                Comunicacion = "No se pudo agregar la dirección",
                TraceIdentifier = HttpContext.TraceIdentifier
            };
            return View("Errores/ErrorMejorado", vm);
        }
    }

    

        public async Task<IActionResult> Direcciones()
        {
        // se trae la clave guid del usuario logueado
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //verificacion
            if (string.IsNullOrEmpty(idUsuarioClaim) || !Guid.TryParse(idUsuarioClaim, out Guid idUsuario))
            {
                return Unauthorized();
            }
            // se envia la clave guid del usuario logueado para traer sus respectivas direcciones
            var direcciones = await _security.GetDireccionesByUsuarioIdAsync(idUsuario);
            return View(direcciones);
        }
    [HttpPost]
    public async Task<IActionResult> EliminarDireccion(int direccionId)
    {
        try
        {
            // Lógica para eliminar la dirección
            await _security.EliminarDireccionAsync(direccionId);
            _logger.LogInformation("Dirección eliminada exitosamente.");
            return RedirectToAction("Direcciones");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar eliminar la dirección.");
            // Manejo de errores
            return RedirectToAction("Direcciones"); // O redirige a una vista de error personalizada
        }
    }






}

