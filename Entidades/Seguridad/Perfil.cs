using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Seguridad;

public class Perfil
{
  public int ID { get; set; }

  public string Nombre { get; set; }

  /// <summary>
  /// El tipo de usuario al cual se aplicara este perfil
  /// </summary>
  public TipoUsuario TipoUsuario { get; set; }

  public string Descripcion { get; set; }
}
