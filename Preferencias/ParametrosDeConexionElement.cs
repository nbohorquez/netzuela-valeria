namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;                         // ConfigurationElement
    using System.Linq;
    using System.Text;
        
    using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

    public class ParametrosDeConexionElement : ConfigurationElement
    {
        /*
         * Referencia: http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.aspx
         */

        #region Constructores

        public ParametrosDeConexionElement() 
        { 
        }

        public ParametrosDeConexionElement(ParametrosDeConexion parametros)
        {
            this.Anfitrion = parametros.Anfitrion;
            this.Servidor = parametros.Servidor;
            this.Instancia = parametros.Instancia;
            this.MetodoDeConexion = parametros.MetodoDeConexion;
            this.ArgumentoDeConexion = parametros.ArgumentoDeConexion;
        }

        #endregion

        #region Propiedades

        [ConfigurationProperty("id", IsRequired = true)]
        public string ID
        {
            get { return (string)base["id"]; }
            set { base["id"] = value; }
        }

        [ConfigurationProperty("anfitrion", IsRequired = true)]
        public string Anfitrion
        {
            get { return (string)base["anfitrion"]; }
            set { base["anfitrion"] = value; }
        }

        [ConfigurationProperty("servidor", IsRequired = true)]
        public string Servidor
        {
            get { return (string)base["servidor"]; }
            set { base["servidor"] = value; }
        }

        [ConfigurationProperty("instancia", IsRequired = true)]
        public string Instancia
        {
            get { return (string)base["instancia"]; }
            set { base["instancia"] = value; }
        }

        [ConfigurationProperty("metodoDeConexion", IsRequired = false)]
        public string MetodoDeConexion
        {
            get { return (string)base["metodoDeConexion"]; }
            set { base["metodoDeConexion"] = value; }
        }

        [ConfigurationProperty("argumentoDeConexion", IsRequired = false)]
        public string ArgumentoDeConexion
        {
            get { return (string)base["argumentoDeConexion"]; }
            set { base["argumentoDeConexion"] = value; }
        }

        #endregion

        #region Funciones

        public ParametrosDeConexion ConvertirAParametrosDeConexion()
        {
            ParametrosDeConexion resultado = new ParametrosDeConexion();

            resultado.Anfitrion = this.Anfitrion;
            resultado.Servidor = this.Servidor;
            resultado.Instancia = this.Instancia;
            resultado.MetodoDeConexion = this.MetodoDeConexion;
            resultado.ArgumentoDeConexion = this.ArgumentoDeConexion;

            return resultado;
        }

        #endregion
    }
}