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
        private readonly Conexion _Conexion;

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

        #endregion

        #region Propiedades

        public IBaseDeDatos BD
        {
            get { return _Conexion.BD; }
        }

        public ConnectionState Estado
        {
            get { return BD.Estado; }
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

        public string BotonConectarDesconectar
        {
            get { return (Estado == ConnectionState.Open) ? "Desconectar" : "Conectar"; }
        }

        public ICommand ConectarDesconectarOrden
        {
            get { return _ConectarDesconectarOrden ?? (_ConectarDesconectarOrden = new RelayCommand(this.ConectarDesconectar)); }
        }

        #endregion

        #region Funciones

        public virtual void ConectarDesconectar()
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

        public void EnCambioDeEstado(object Remitente, StateChangeEventArgs Argumentos)
        {
            RaisePropertyChanged("Estado");                     // Para la gente de MainViewModel
            RaisePropertyChanged("EstadoString");               // Para la gente de BarraDeEstadoView
            RaisePropertyChanged("BotonConectarDesconectar");   // Para la gente de ConexionLocalView y ConexionRemotaView
        }

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            try
            {
                ResolverDatosDeConexion();
                BD.EnCambioDeEstado = new StateChangeEventHandler(EnCambioDeEstado);
                _Conexion.Conectar(Usuario, Contrasena);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
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
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        public void CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] ColumnasAutorizadas, int Privilegios)
        {
            try
            {
                _Conexion.CrearUsuario(Usuario, Contrasena, ColumnasAutorizadas, Privilegios);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
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
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        #endregion
    }
}
