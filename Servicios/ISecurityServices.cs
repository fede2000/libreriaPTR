using Entidades.Seguridad;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicios;

public interface ISecurityServices
{
  IEnumerable<Perfil> GetPerfiles();

  Usuario CrearEmpleado(string nombre, string mail, string login, string pwd, DateTime nacimiento,
    int[] perfiles);

  Usuario CrearCliente(string nombre, string mail, string login, string pwd, DateTime nacimiento);

  bool SetearPassword(Guid id, string newPass);

  Usuario Login(string login, string pass);

  Task<Direccion> CrearDireccionAsync(string linea1, string linea2, string codigoPostal, string provincia, string localidad, string pais, Guid idUsuario);

    Task<IEnumerable<Direccion>> GetDireccionesByUsuarioIdAsync(Guid idUsuario);

    Task EliminarDireccionAsync(int direccionId);

}
