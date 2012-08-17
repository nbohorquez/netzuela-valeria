﻿namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;         // ConfigurationElement
    using System.Linq;
    using System.Text;

    public class UsuarioContrasenaElement : ConfigurationElement
    {
        #region Constructores

        public UsuarioContrasenaElement() 
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

        [ConfigurationProperty("llave", IsRequired = true)]
        public string Llave
        {
            get { return (string)base["llave"]; }
            set { base["llave"] = value; }
        }

        /*
        [ConfigurationProperty("usuario", IsRequired = true)]
        public string Usuario
        {
            get { return (string)base["usuario"]; }
            set { base["usuario"] = value; }
        }

        [ConfigurationProperty("contrasena", IsRequired = true)]
        public string Contrasena
        {
            get { return (string)base["contrasena"]; }
            set { base["contrasena"] = value; }
        }
        */
        #endregion
    }
}