using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;     // ConfigurationSection

namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    public class ConexionesSection : ConfigurationSection
    {
        // Con codigo de http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx

        #region Constructores

        public ConexionesSection() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("parametrosDeConexion")]
        public ColeccionElementosGenerica<ParametrosDeConexionElement> ParametrosDeConexion
        {
            get { return (ColeccionElementosGenerica<ParametrosDeConexionElement>)this["parametrosDeConexion"]; }
            set { this["parametrosDeConexion"] = value; }
        }

        #endregion
    }
}
