namespace Zuliaworks.Netzuela.Spuria.ServidorOAuth.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class AutorizacionModel
    {
        #region Propiedades

        /// <summary>
        /// Nombre de la aplicación que realiza la petición de autorización
        /// </summary>
        public string AplicacionConsumidora { get; set; }

        /// <summary>
        /// Indica si la solicitud fue aprobada o no
        /// </summary>
        public bool Aprobado { get; set; }

        /// <summary>
        /// Indica si la petición fue hecha desde un entorno seguro
        /// </summary>
        public bool PeticionInsegura { get; set; }

        /// <summary>
        /// Código que debe suministrar el usuario a la aplicación consumidora para 
        /// permitirle acceso a sus datos privados en el proveedor
        /// </summary>
        public string CodigoDeVerificacion { get; set; }

        #endregion
    }
}