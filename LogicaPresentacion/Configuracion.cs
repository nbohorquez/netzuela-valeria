using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    public class Configuracion
    {
        #region Constructores

        public Configuracion()
        {
            Mapas = new List<string[]>();
        }

        #endregion

        #region Propiedades

        public ParametrosDeConexion ParametrosConexionLocal { get; set; }
        public ParametrosDeConexion ParametrosConexionRemota { get; set; }
        public SecureString UsuarioLocal { get; set; }
        public SecureString ContrasenaLocal { get; set; }
        public SecureString UsuarioRemoto { get; set; }
        public SecureString ContrasenaRemota { get; set; }
        public List<string[]> Mapas { get; set; }

        #endregion
    }
}