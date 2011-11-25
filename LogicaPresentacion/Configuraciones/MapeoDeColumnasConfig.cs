using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;         // ConfigurationElement

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    public class MapeoDeColumnasConfig : ConfigurationElement
    {
        #region Constructores

        public MapeoDeColumnasConfig() { }

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
