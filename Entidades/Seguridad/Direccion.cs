using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Seguridad
{
	public class Direccion
	{
		public int ID { get; set; }

		public string Linea1 { get; set; }

		public string Linea2 { get; set; }

		public string Codigo_Postal { get; set;}

		public string Localidad { get; set; }

		public string Provincia { get; set; }

		public string Pais { get; set;}

		public bool Verificada { get; set; }

		public Guid ID_Usuario { get; set; }
        public Usuario Usuario { get; set; }
    }
}
