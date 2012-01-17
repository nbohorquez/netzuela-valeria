using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand, ObservableObject
using ObviexGeneradorContrasenas;               // RandomPassword
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
    public class ConexionLocalViewModel : ConexionViewModel
    {
        #region Variables

        private RelayCommand _DetectarOrden;
        private bool _MostrarAutentificacionView;
        private bool _MostrarDetectarServidoresLocalesView;
        private PropertyObserver<AutentificacionViewModel> _ObservadorAutentificacion;
        private PropertyObserver<DetectarServidoresLocalesViewModel> _ObservadorServidores;
        private AutentificacionViewModel _Autentificacion;
        private DetectarServidoresLocalesViewModel _ServidoresDetectados;
        private SecureString _UsuarioExterno;
        private SecureString _ContrasenaExterna;

        #endregion

        #region Contructores

        public ConexionLocalViewModel()
            : base() { }

        #endregion

        #region Propiedades

        public SecureString UsuarioNetzuela { get; private set; }
        public SecureString ContrasenaNetzuela { get; private set; }

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
            get { return _DetectarOrden ?? (_DetectarOrden = new RelayCommand(this.AbrirDetectarServidores)); }
        }

        #endregion

        #region Funciones

        private void AbrirDetectarServidores()
        {
            ServidoresDetectados = new DetectarServidoresLocalesViewModel();

            _ObservadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.ServidoresDetectados)
                .RegisterHandler(n => n.MostrarView, this.CerrarDetectarServidores);

            MostrarDetectarServidoresLocalesView = true;
        }

        private void CerrarDetectarServidores(DetectarServidoresLocalesViewModel ServidoresVM)
        {
            if (ServidoresVM.MostrarView == false)
            {
                MostrarDetectarServidoresLocalesView = false;
                Parametros = ServidoresVM.Parametros.Clonar();
            }
        }

        private void AbrirAutentificacion()
        {
            Autentificacion = new AutentificacionViewModel();

            _ObservadorAutentificacion = new PropertyObserver<AutentificacionViewModel>(this.Autentificacion)
                .RegisterHandler(n => n.MostrarView, this.CerrarAutentificacion);
            
            MostrarAutentificacionView = true;
        }

        private void CerrarAutentificacion(AutentificacionViewModel AutentificacionVM)
        {
            if (AutentificacionVM.MostrarView == false)
            {
                MostrarAutentificacionView = false;

                _UsuarioExterno = AutentificacionVM.Usuario.Copy();
                _ContrasenaExterna = AutentificacionVM.Contrasena.Copy();

                try
                {
                    ConexionUsuario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            }
        }

        private void ConexionUsuario()
        {
            base.Conectar(_UsuarioExterno, _ContrasenaExterna);

            // Borramos todo aquello que pudiese resultar atractivo para alguien con malas intenciones
            if (_UsuarioExterno != null)
            {
                _UsuarioExterno.Dispose();
                Autentificacion.Usuario = null;
                _UsuarioExterno = null;
            }
            if (_ContrasenaExterna != null)
            {
                _ContrasenaExterna.Dispose();
                Autentificacion.Contrasena = null;
                _ContrasenaExterna = null;
            }
        }

        protected override void ConectarDesconectar()
        {
            if (Estado == ConnectionState.Open)
            {
                base.Desconectar();
            }
            else
            {
                AbrirAutentificacion();
            }
        }
            
        public void ConexionNetzuela()
        {
            try
            {
                base.Conectar(UsuarioNetzuela, ContrasenaNetzuela);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al tratar de establecer la conexión local con el usuario Netzuela", ex);
            }
        }
        
        public bool CrearUsuarioNetzuela(string[] ColumnasAutorizadas)
        {
            bool Resultado = false;
            // ¿Será un problema de seguridad grave colocar el nombre "netzuela" asi tan a la vista?
            UsuarioNetzuela = "netzuela".ConvertirASecureString();
            ContrasenaNetzuela = RandomPassword.Generate(20).ConvertirASecureString();

            try
            {
                Resultado = _Conexion.CrearUsuario(UsuarioNetzuela, ContrasenaNetzuela, ColumnasAutorizadas, Constantes.Privilegios.SELECCIONAR);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario Netzuela", ex);
            }

            return Resultado;
        }

        #endregion
    }
}
