using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Data;                              // DataTable, ConnectionState
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexionRemotaViewModel : ConexionViewModel
    {
        /*
         * Esto esta parapeteado nada mas, hay que acomodarlo bien cuando se tenga el servidor de 
         * Netzuela operativo
         */

        #region Constructores

        public ConexionRemotaViewModel()
            : base() { }

        public ConexionRemotaViewModel(Conexion Conexion)
            : base(Conexion) { }

        public ConexionRemotaViewModel(ParametrosDeConexion Parametros)
            : base(Parametros) { }

        #endregion        
    }
}
