using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;         // ConfigurationElement

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Preferencias
{
    public class TablasMapeadasSection : ConfigurationSection
    {
        #region Constructores

        public TablasMapeadasSection() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("tablas")]
        public ColeccionElementosGenerica<TablaMapeadaElement> Tablas
        {
            get { return (ColeccionElementosGenerica<TablaMapeadaElement>)this["tablas"]; }
            set { this["tablas"] = value; }
        }

        #endregion
    }
}
