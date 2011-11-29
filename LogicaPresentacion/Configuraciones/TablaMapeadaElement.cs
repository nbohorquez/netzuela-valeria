using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;         // ConfigurationElement

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    public class TablaMapeadaElement : ConfigurationElement
    {
        #region Constructores

        public TablaMapeadaElement() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("tablaMapeada")]
        [ConfigurationCollection(typeof(MapeoDeColumnasElement), 
            AddItemName = "add", 
            ClearItemsName = "clear", 
            RemoveItemName = "remove")]
        public ColeccionElementosGenerica<MapeoDeColumnasElement> TablaMapeada
        {
            get { return (ColeccionElementosGenerica<MapeoDeColumnasElement>)base["tablaMapeada"]; }
            set { base["tablaMapeada"] = value; }
        }

        #endregion
    }
}
