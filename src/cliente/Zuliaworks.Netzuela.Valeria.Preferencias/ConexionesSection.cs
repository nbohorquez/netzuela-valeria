namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;     // ConfigurationSection
    using System.Linq;
    using System.Text;
    
    public class ConexionesSection : ConfigurationSection
    {
        /*
         * Referencia: http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx
         */

        #region Constructores

        public ConexionesSection() 
        { 
        }

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
