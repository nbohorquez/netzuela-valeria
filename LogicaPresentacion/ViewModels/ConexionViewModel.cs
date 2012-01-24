using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Data;                              // DataTable, ConnectionState
using System.Windows.Input;                     // ICommand
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class ConexionViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _ConectarDesconectarOrden;
        private bool _PermitirModificaciones;
        protected Conexion _Conexion;

        #endregion

        #region Constructores

        public ConexionViewModel()
        {
            _Conexion = new Conexion();
        }

        public ConexionViewModel(Conexion Conexion)
        {
            if (Conexion == null)
                throw new ArgumentNullException("Conexion");

            _Conexion = Conexion;
        }

        public ConexionViewModel(ParametrosDeConexion Parametros)
        {
            if (Parametros == null)
                throw new ArgumentNullException("Parametros");

            _Conexion = new Conexion(Parametros);
        }

        #endregion

        #region Propiedades

        public Conexion Conexion
        {
            get { return _Conexion; }
        }

        public ConnectionState Estado
        {
            get { return _Conexion.Estado; }
        }

        public ParametrosDeConexion Parametros
        {
            get { return _Conexion.Parametros; }
            set
            {
                if (value != _Conexion.Parametros)
                {
                    _Conexion.Parametros = value;
                    RaisePropertyChanged("Parametros");
                }
            }
        }

        public string EstadoString
        {
            get
            {
                string Resultado;

                switch (Estado)
                {
                    case ConnectionState.Broken:
                        Resultado = "Rota";
                        break;
                    case ConnectionState.Closed:
                        Resultado = "Cerrada";
                        break;
                    case ConnectionState.Connecting:
                        Resultado = "Conectando...";
                        break;
                    case ConnectionState.Executing:
                        Resultado = "Ejecutando";
                        break;
                    case ConnectionState.Fetching:
                        Resultado = "Recibiendo";
                        break;
                    case ConnectionState.Open:
                        Resultado = "Establecida";
                        break;
                    default:
                        Resultado = "Indeterminada";
                        break;
                }

                return Resultado;
            }
        }

        public bool PermitirModificaciones
        {
            get { return (Estado == ConnectionState.Open) ? false : true; }
        }

        public string BotonConectarDesconectar
        {
            get { return (Estado == ConnectionState.Open) ? "Desconectar" : "Conectar"; }
        }

        public ICommand ConectarDesconectarOrden
        {
            get { return _ConectarDesconectarOrden ?? (_ConectarDesconectarOrden = new RelayCommand(this.ConectarDesconectarAccion)); }
        }

        #endregion

        #region Funciones

        private void ConectarDesconectarAccion()
        {
            try
            {
                ConectarDesconectar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected virtual void ConectarDesconectar()
        {
            if (Estado == ConnectionState.Open)
            {
                Desconectar();
            }
            else
            {
                Conectar(null, null);
            }            
        }

        public void ManejarCambioDeEstado(object Remitente, StateChangeEventArgs Argumentos)
        {            
            RaisePropertyChanged("Estado");                     // Para la gente de MainViewModel
            RaisePropertyChanged("EstadoString");               // Para la gente de BarraDeEstadoView
            RaisePropertyChanged("BotonConectarDesconectar");   // Para la gente de ConexionLocalView y ConexionRemotaView
            RaisePropertyChanged("PermitirModificaciones");     // Para la gente de ConexionLocalView y ConexionRemotaView
        }

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            ResolverDatosDeConexion();
            
            try
            {
                // Esto es para evitar que aparezca registrado el mismo manejador dos veces
                _Conexion.CambioDeEstado -= ManejarCambioDeEstado;
                _Conexion.CambioDeEstado += ManejarCambioDeEstado;

                _Conexion.Conectar(Usuario, Contrasena);
            }
            catch (Exception ex)
            {
                throw new Exception("Error de conexión", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                _Conexion.Desconectar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error de desconexión", ex);
            }
        }

        public void ResolverDatosDeConexion()
        {
            try
            {
                _Conexion.ResolverDatosDeConexion();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al tratar de resolver los datos de conexión", ex);
            }
        }

        #endregion
    }
}
