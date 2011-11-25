using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security;          // SecureString
using System.Configuration;     // ConfigurationSection

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    public class AutentificacionConfig : ConfigurationSection
    {
        #region Constructores

        public AutentificacionConfig() { }

        #endregion

        #region Propiedades

        [ConfigurationProperty("usuarioLocal", IsRequired = true)]
        public string UsuarioLocal
        {
            get { return (string)base["usuarioLocal"]; }
            set { base["usuarioLocal"] = value; }
        }

        [ConfigurationProperty("contrasenaLocal", IsRequired = true)]
        public string ContrasenaLocal
        {
            get { return (string)base["contrasenaLocal"]; }
            set { base["contrasenaLocal"] = value; }
        }

        [ConfigurationProperty("usuarioRemoto", IsRequired = true)]
        public string UsuarioRemoto
        {
            get { return (string)base["usuarioRemoto"]; }
            set { base["usuarioRemoto"] = value; }
        }

        [ConfigurationProperty("contrasenaRemota", IsRequired = true)]
        public string ContrasenaRemota
        {
            get { return (string)base["contrasenaRemota"]; }
            set { base["contrasenaRemota"] = value; }
        }

        #endregion
    }
}
