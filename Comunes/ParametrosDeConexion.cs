using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Esta clase se usa en todas partes.
    /// </summary>
    public class ParametrosDeConexion
    {
        #region Constructores

        public ParametrosDeConexion() 
        {
            Anfitrion = string.Empty;
            Servidor = string.Empty;
            Instancia = string.Empty;
            MetodoDeConexion = string.Empty;
            ArgumentoDeConexion = string.Empty;
        }

        #endregion

        #region Propiedades

        public string Anfitrion { get; set; }
        public string Servidor { get; set; }
        public string Instancia { get; set; }
        public string MetodoDeConexion { get; set; }
        public string ArgumentoDeConexion { get; set; }

        #endregion

        #region Funciones

        public ParametrosDeConexion Clonar()
        {
            ParametrosDeConexion Resultado = new ParametrosDeConexion();

            if(this.Anfitrion != null)
                Resultado.Anfitrion = string.Copy(this.Anfitrion);

            if(this.Servidor != null)
                Resultado.Servidor = string.Copy(this.Servidor);

            if(this.Instancia != null)
                Resultado.Instancia = string.Copy(this.Instancia);

            if(this.MetodoDeConexion != null)
                Resultado.MetodoDeConexion = string.Copy(this.MetodoDeConexion);

            if(this.ArgumentoDeConexion != null)
                Resultado.ArgumentoDeConexion = string.Copy(this.ArgumentoDeConexion);

            return Resultado;
        }

        #endregion   
    }
}
