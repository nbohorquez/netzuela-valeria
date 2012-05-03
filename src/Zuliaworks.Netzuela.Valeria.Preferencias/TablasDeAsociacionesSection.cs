namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;         // ConfigurationElement
    using System.Linq;
    using System.Text;

    public class TablasDeAsociacionesSection : ConfigurationSection
    {
        #region Constructores

        public TablasDeAsociacionesSection() 
        { 
        }

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
