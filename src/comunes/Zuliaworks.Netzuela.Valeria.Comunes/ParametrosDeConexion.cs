//-----------------------------------------------------------------------
// <copyright file="ParametrosDeConexion.cs" company="Zuliaworks">
//     Copyright (c) Zuliaworks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Esta clase se usa en todas partes.
    /// </summary>
    public class ParametrosDeConexion
    {
        #region Constructores

        public ParametrosDeConexion() 
        {
            this.Anfitrion = string.Empty;
            this.Servidor = string.Empty;
            this.Instancia = string.Empty;
            this.MetodoDeConexion = string.Empty;
            this.ArgumentoDeConexion = string.Empty;
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
            ParametrosDeConexion resultado = new ParametrosDeConexion();

            if (this.Anfitrion != null)
            {
                resultado.Anfitrion = string.Copy(this.Anfitrion);
            }

            if (this.Servidor != null)
            {
                resultado.Servidor = string.Copy(this.Servidor);
            }

            if (this.Instancia != null)
            {
                resultado.Instancia = string.Copy(this.Instancia);
            }

            if (this.MetodoDeConexion != null)
            {
                resultado.MetodoDeConexion = string.Copy(this.MetodoDeConexion);
            }

            if (this.ArgumentoDeConexion != null)
            {
                resultado.ArgumentoDeConexion = string.Copy(this.ArgumentoDeConexion);
            }

            return resultado;
        }

        #endregion   
    }
}
