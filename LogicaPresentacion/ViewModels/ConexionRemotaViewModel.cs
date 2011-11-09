using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Data;                              // DataTable, ConnectionState
using System.Windows;                           // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexionRemotaViewModel : ObservableObject
    {
        /*
         * Esto esta parapeteado nada mas, hay que acomodarlo bien cuando se tenga el servidor de 
         * Netzuela operativo
         */

        #region Variables

        private Conexion _Remota;
        
        #endregion

        #region Constructores

        public ConexionRemotaViewModel()
        {
            _Remota = new Conexion(new DatosDeConexion() { Servidor = Constantes.SGBDR.NETZUELA, Instancia = "Isla Providencia" });

            try
            {
                BD.EnCambioDeEstado = new StateChangeEventHandler(EnCambioDeEstado);
                _Remota.Conectar(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.InnerException);
            }
        }

        #endregion

        #region Propiedades

        public IBaseDeDatos BD
        {
            get { return _Remota.BD; }
        }

        public ConnectionState Estado
        {
            get { return BD.Estado; }
        }

        #endregion

        #region Funciones

        private void EnCambioDeEstado(object Remitente, StateChangeEventArgs Argumentos)
        {
            RaisePropertyChanged("Estado");
        }

        #endregion
    }
}
