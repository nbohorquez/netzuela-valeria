namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;         // ConfigurationElement
    using System.Linq;
    using System.Text;

    public class TablaDeAsociacionesElement : ConfigurationElement
    {
        #region Constructores

        public TablaDeAsociacionesElement() 
        { 
        }

        #endregion

        #region Propiedades

        [ConfigurationProperty("id", IsRequired = true)]
        public string Id
        {
            get { return (string)base["id"]; }
            set { base["id"] = value; }
        }

        [ConfigurationProperty("tablaMapeada")]
        [ConfigurationCollection(typeof(AsociacionDeColumnasElement),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ColeccionElementosGenerica<AsociacionDeColumnasElement> TablaMapeada
        {
            get { return (ColeccionElementosGenerica<AsociacionDeColumnasElement>)base["tablaMapeada"]; }
            set { base["tablaMapeada"] = value; }
        }

        #endregion
    }
}