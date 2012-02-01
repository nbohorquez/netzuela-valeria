using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;         // ConfigurationElement

namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    public class TablasDeAsociacionesSection : ConfigurationSection
    {
        #region Constructores

        public TablasDeAsociacionesSection() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("tablas")]
        public ColeccionElementosGenerica<TablaDeAsociacionesElement> Tablas
        {
            get { return (ColeccionElementosGenerica<TablaDeAsociacionesElement>)this["tablas"]; }
            set { this["tablas"] = value; }
        }

        #endregion
    }
}
