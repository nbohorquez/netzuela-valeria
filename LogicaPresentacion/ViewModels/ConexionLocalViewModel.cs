using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand, ObservableObject
using System.Data;                              // DataTable, ConnectionState
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexionLocalViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _DetectarOrden;
        private RelayCommand _ConectarOrden;
        private RelayCommand _DesconectarOrden;
        private bool _MostrarAutentificacionView;
        private bool _MostrarDetectarServidoresLocalesView;
        private PropertyObserver<AutentificacionViewModel> _ObservadorAutentificacion;
        private PropertyObserver<DetectarServidoresLocalesViewModel> _ObservadorServidores;
        private AutentificacionViewModel _Autentificacion;
        private DetectarServidoresLocalesViewModel _ServidoresDetectados;
        private SecureString _Usuario;
        private SecureString _Contrasena;
        private Conexion _Local;

        #endregion

        #region Contructores

        public ConexionLocalViewModel()
        {
            _Local = new Conexion();
        }

        #endregion

        #region Propiedades

        public IBaseDeDatos BD
        {
            get { return _Local.BD; }
        }

        public ConnectionState Estado
        { 
            get { return BD.Estado; }
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

        public DatosDeConexion Datos
        {
            get { return _Local.Datos; }
            set
            {
                if (value != _Local.Datos)
                {
                    _Local.Datos = value;
                    RaisePropertyChanged("Datos");
                }
            }
        }
        
        public AutentificacionViewModel Autentificacion
        {
            get { return _Autentificacion; }
            private set
            {
                if (value != _Autentificacion)
                {
                    _Autentificacion = value;
                    RaisePropertyChanged("Autentificacion");
                }
            }
        }

        public DetectarServidoresLocalesViewModel ServidoresDetectados
        {
            get { return _ServidoresDetectados; }
            set
            {
                if (value != _ServidoresDetectados)
                {
                    _ServidoresDetectados = value;
                    RaisePropertyChanged("ServidoresDetectados");
                }
            }
        }

        public bool MostrarAutentificacionView
        {
            get { return _MostrarAutentificacionView; }
            set 
            {
                if (value != _MostrarAutentificacionView)
                {
                    _MostrarAutentificacionView = value;
                    RaisePropertyChanged("MostrarAutentificacionView");
                }
            }
        }

        public bool MostrarDetectarServidoresLocalesView
        {
            get { return _MostrarDetectarServidoresLocalesView; }
            set
            {
                if (value != _MostrarDetectarServidoresLocalesView)
                {
                    _MostrarDetectarServidoresLocalesView = value;
                    RaisePropertyChanged("MostrarDetectarServidoresLocalesView");
                }
            }
        }

        public ICommand DetectarOrden
        {
            get { return _DetectarOrden ?? (_DetectarOrden = new RelayCommand(this.AbrirDetectarServidoresLocalesView)); }
        }

        public ICommand ConectarOrden
        {
            get { return _ConectarOrden ?? (_ConectarOrden = new RelayCommand(this.AbrirAutentificacionView)); }
        }

        public ICommand DesconectarOrden
        {
            get { return _DesconectarOrden ?? (_DesconectarOrden = new RelayCommand(this.Desconectar)); }
        }

        #endregion

        #region Funciones

        private void AbrirDetectarServidoresLocalesView()
        {
            ServidoresDetectados = new DetectarServidoresLocalesViewModel();

            _ObservadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.ServidoresDetectados)
                .RegisterHandler(n => n.MostrarView, this.CerrarDetectarServidoresLocalesView);

            MostrarDetectarServidoresLocalesView = true;
        }

        private void CerrarDetectarServidoresLocalesView(DetectarServidoresLocalesViewModel ServidoresVM)
        {
            if (ServidoresVM.MostrarView == false)
            {
                this.MostrarDetectarServidoresLocalesView = false;
                this.Datos = ServidoresVM.Datos;
            }
        }

        private void AbrirAutentificacionView()
        {
            Autentificacion = new AutentificacionViewModel();

            _ObservadorAutentificacion = new PropertyObserver<AutentificacionViewModel>(this.Autentificacion)
                .RegisterHandler(n => n.MostrarView, this.CerrarAutentificacionView);
            
            MostrarAutentificacionView = true;
        }

        private void CerrarAutentificacionView(AutentificacionViewModel AutentificacionVM)
        {
            if (AutentificacionVM.MostrarView == false)
            {
                this.MostrarAutentificacionView = false;
                this._Usuario = AutentificacionVM.Usuario;
                this._Contrasena = AutentificacionVM.Contrasena;
                Conectar();
            }
        }

        private void EnCambioDeEstado(object Remitente, StateChangeEventArgs Argumentos)
        {
            RaisePropertyChanged("Estado");
            RaisePropertyChanged("EstadoString");
        }

        public void Conectar()
        {
            try
            {
                _Local.ResolverDatosDeConexion();
                BD.EnCambioDeEstado = new StateChangeEventHandler(EnCambioDeEstado);
                _Local.Conectar(_Usuario, _Contrasena);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }
        }

        public void Desconectar()
        {
            _Local.Desconectar();
        }

        #endregion
    }
}
