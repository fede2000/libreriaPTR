using Entidades.Seguridad;
using Entidades;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos.Repositorios;
using Microsoft.Extensions.Logging;
using Utiles;

namespace Servicios;
public class SecurityServices : ISecurityServices
{
    private readonly ISecurityRepo _repo;
    private readonly ILogger<SecurityServices> _logger;
    private readonly GeocodingService _geocodingService;

    public SecurityServices(ISecurityRepo repo, ILogger<SecurityServices> logger, GeocodingService geocodingService)
    {
        _repo = repo;
        _logger = logger;
        _geocodingService = geocodingService;
    }

    public IEnumerable<Perfil> GetPerfiles()
    {
        return _repo.GetPerfiles();
    }

    public Usuario CrearEmpleado(string nombre, string mail, string login, string pwd, DateTime nacimiento,
      int[] perfiles)
    {
        Usuario nuevo = new Usuario
        {
            Nombre = nombre,
            Login = login,
            Correo = mail,
            Nacimiento = nacimiento,
            TipoUsuario = TipoUsuario.Empleado,
            FechaAlta = DateTime.Now
        };

        foreach (var perfil in perfiles)
            nuevo.Perfiles.Add(GetPerfiles().Single(p => p.ID == perfil));

        return _repo.CrearUsuario(nuevo, pwd);
    }

    public Usuario CrearCliente(string nombre, string mail, string login, string pwd, DateTime nacimiento)
    {
        Usuario nuevo = new Usuario
        {
            Nombre = nombre,
            Login = login,
            Correo = mail,
            Nacimiento = nacimiento,
            TipoUsuario = TipoUsuario.Cliente,
            FechaAlta = DateTime.Now
        };

        nuevo.Perfiles.Add(GetPerfiles().Single(p => p.Nombre == "Visitante"));

        return _repo.CrearUsuario(nuevo, pwd);
    }

    public bool SetearPassword(Guid id, string newPass)
    {
        //  que chequeos podriamos hacer?
        return true;
    }

    public Usuario Login(string login, string pass)
    {
        var user = _repo.GetUsuarioFromLogin(login);

        if (user != null)
        {
            if (!_repo.ValidarPassword(user.Clave, pass))
                throw new ApplicationException("Pass invalida...POSIBLE FRAUDE??? INCREMENTAR REINTENTOS");

            return user;
        }

        _logger.LogCritical("El usuario {login} no existe. Chequear fraudes", login);
        throw new ApplicationException($"El usuario {login} no existe. Chequear fraudes");
    }

    public async Task<Direccion> CrearDireccionAsync(string linea1, string linea2, string codigoPostal, string provincia, string localidad, string pais, Guid idUsuario)
    {
        string direccionCompleta = $"{linea1}, {linea2}, {codigoPostal}, {localidad}, {provincia}, {pais}";
        // Se envia los datos de la nueva direccion a la Api de geocoding para validar
        bool esValida = await _geocodingService.VerificarDireccionAsync(direccionCompleta);
        // validadcion de la respuesta de la api Geocoding
        if (!esValida)
        {
            throw new ApplicationException("La dirección proporcionada no es válida.");
        }
        // la direccion paso la validacion se procede a crear
        Direccion nuevo = new Direccion
        {
            Linea1 = linea1,
            Linea2 = linea2,
            Codigo_Postal = codigoPostal,
            Localidad = localidad,
            Provincia = provincia,
            Pais = pais,
            Verificada = true,
            ID_Usuario = idUsuario
        };
        // se envia la direccion para agregarla a la base de datos a traves de _repo.agregarDireccionAsync

        return await _repo.AgregarDireccionAsync(nuevo);
    }

    public async Task<IEnumerable<Direccion>> GetDireccionesByUsuarioIdAsync(Guid idUsuario)
    {
        //se envia la clave guid del usuario logueado para obtener las direcciones
        return await _repo.GetDireccionesByUsuarioIdAsync(idUsuario);
    }

    public async Task EliminarDireccionAsync(int direccionId)
    {
        await _repo.EliminarDireccionAsync(direccionId);
    }
}
