﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;     // ConfigurationSection

namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    public class AutentificacionSection : ConfigurationSection
    {
        // Referencia: http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx

        #region Constructores

        public AutentificacionSection() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("llavesDeAcceso")]
        public ColeccionElementosGenerica<UsuarioContrasenaElement> LlavesDeAcceso
        {
            get { return (ColeccionElementosGenerica<UsuarioContrasenaElement>)this["llavesDeAcceso"]; }
            set { this["llavesDeAcceso"] = value; }
        }

        #endregion
    }
}
