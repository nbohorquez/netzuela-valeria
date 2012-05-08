using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Data;                              // DataTable, ConnectionState
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexionRemotaViewModel : ConexionViewModel, IDisposable
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

        ~ConexionRemotaViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            if (borrarCodigoAdministrado)
            {
                if (this._Conexion != null)
                {
                    this._Conexion.Dispose();
                    this._Conexion = null;
                }
            }
        }

        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
