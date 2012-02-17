namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;         // ConfigurationElement
    using System.Linq;
    using System.Text;

    public class AsociacionDeColumnasElement : ConfigurationElement
    {
        /*
         * Referencia: http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx
         */

        #region Constructores

        public AsociacionDeColumnasElement() 
        { 
        }

        #endregion

        #region Propiedades

        [ConfigurationProperty("nodoOrigen", IsRequired = true)]
        public string NodoOrigen
        {
            get { return (string)base["nodoOrigen"]; }
            set { base["nodoOrigen"] = value; }
        }

        [ConfigurationProperty("nodoDestino", IsRequired = true)]
        public string NodoDestino
        {
            get { return (string)base["nodoDestino"]; }
            set { base["nodoDestino"] = value; }
        }

        #endregion
    }
}
