using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;     // ConfigurationSection

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    public class ConexionesSection : ConfigurationSection
    {
        // Con codigo de http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx

        #region Constructores

        public ConexionesSection() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("parametrosConexionLocal", IsRequired = false)]
        public ParametrosDeConexionElement ParametrosConexionLocal
        {
            get { return (ParametrosDeConexionElement)base["parametrosConexionLocal"]; }
            set { base["parametrosConexionLocal"] = value; }
        }

        [ConfigurationProperty("parametrosConexionRemota", IsRequired = false)]
        public ParametrosDeConexionElement ParametrosConexionRemota
        {
            get { return (ParametrosDeConexionElement)base["parametrosConexionRemota"]; }
            set { base["parametrosConexionRemota"] = value; }
        }

        #endregion
    }
}
