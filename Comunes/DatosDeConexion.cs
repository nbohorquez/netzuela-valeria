using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Esta clase se usa en todas partes.
    /// </summary>
    public class DatosDeConexion
    {
        #region Constructores

        public DatosDeConexion() 
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

        public DatosDeConexion Clonar()
        {
            DatosDeConexion Resultado = new DatosDeConexion();

            Resultado.Anfitrion = string.Copy(this.Anfitrion);
            Resultado.Servidor = string.Copy(this.Servidor);
            Resultado.Instancia = string.Copy(this.Instancia);
            Resultado.MetodoDeConexion = string.Copy(this.MetodoDeConexion);
            Resultado.ArgumentoDeConexion = string.Copy(this.ArgumentoDeConexion);

            return Resultado;
        }

        #endregion   
    }
}
