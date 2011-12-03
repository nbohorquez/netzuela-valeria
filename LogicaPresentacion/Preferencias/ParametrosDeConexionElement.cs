using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;                         // ConfigurationElement
using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Preferencias
{
    public class ParametrosDeConexionElement : ConfigurationElement
    {
        // Con codigo de http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration.asp

        #region Constructores

        public ParametrosDeConexionElement() { }

        public ParametrosDeConexionElement(ParametrosDeConexion Parametros) 
        {
            this.Anfitrion = Parametros.Anfitrion;
            this.Servidor = Parametros.Servidor;
            this.Instancia = Parametros.Instancia;
            this.MetodoDeConexion = Parametros.MetodoDeConexion;
            this.ArgumentoDeConexion = Parametros.ArgumentoDeConexion;
        }

        #endregion

        #region Propiedades

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
            ParametrosDeConexion Resultado = new ParametrosDeConexion();

            Resultado.Anfitrion = this.Anfitrion;
            Resultado.Servidor = this.Servidor;
            Resultado.Instancia = this.Instancia;
            Resultado.MetodoDeConexion = this.MetodoDeConexion;
            Resultado.ArgumentoDeConexion = this.ArgumentoDeConexion;

            return Resultado;
        }

        #endregion
    }
}
