using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;     // ConfigurationSection

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    public class ConexionesConfig : ConfigurationSection
    {
        // Con codigo de http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx

        #region Constructores

        public ConexionesConfig() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("parametrosConexionLocal", IsRequired = false)]
        public ParametrosDeConexionConfig ParametrosConexionLocal
        {
            get { return (ParametrosDeConexionConfig)base["parametrosConexionLocal"]; }
            set { base["parametrosConexionLocal"] = value; }
        }

        [ConfigurationProperty("parametrosConexionRemota", IsRequired = false)]
        public ParametrosDeConexionConfig ParametrosConexionRemota
        {
            get { return (ParametrosDeConexionConfig)base["parametrosConexionRemota"]; }
            set { base["parametrosConexionRemota"] = value; }
        }

        #endregion
    }
}
