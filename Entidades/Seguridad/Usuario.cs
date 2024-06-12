using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Seguridad;

public class Usuario
{
  public Usuario()
  {
        //el hashSet se asegura que el usuario solo tenga un perfil 
    Perfiles = new HashSet<Perfil>();
  }

  public Guid Clave { get; set; }

  public string Login { get; set; }

  public string Nombre { get; set; }

  public TipoUsuario TipoUsuario { get; set; }

  public bool Habilitado { get; set; }

  public string Correo { get; set; }

 

  public DateTime FechaAlta { get; set; }

  public DateTime Nacimiento { get; set; }

  public ISet<Perfil> Perfiles { get; set; }

	public ISet<Direccion> Direcciones { get; set; }

	public DateTime? LastLogin { get; set; }
}
