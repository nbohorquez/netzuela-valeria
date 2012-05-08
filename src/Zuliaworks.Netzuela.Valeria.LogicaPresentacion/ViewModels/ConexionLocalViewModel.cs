namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using MvvmFoundation.Wpf;                                   // RelayCommand, ObservableObject

    using System;
    using System.Collections.Generic;
    using System.Data;                                          // DataTable, ConnectionState
    using System.Linq;
    using System.Security;                                      // SecureString
    using System.Text;
    using System.Windows;                                       // MessageBox
    using System.Windows.Input;                                 // ICommand
    
    using Zuliaworks.Netzuela.Valeria.Comunes;                  // DatosDeConexion, PasswordGenerator
    using Zuliaworks.Netzuela.Valeria.Logica;                   // Conexion

    /// <summary>
    /// 
    /// </summary>
    public class ConexionLocalViewModel : ConexionViewModel, IDisposable
    {
        #region Variables

        private PasswordGenerator _GeneradorDeContrasenas;
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
            : base() 
        {
            InicializarGeneradorDeContrasenas();
        }

        public ConexionLocalViewModel(Conexion Conexion)
            : base(Conexion) 
        {
            InicializarGeneradorDeContrasenas();
        }

        public ConexionLocalViewModel(ParametrosDeConexion Parametros)
            : base(Parametros) 
        {
            InicializarGeneradorDeContrasenas();
        }

        ~ConexionLocalViewModel()
        {
            this.Dispose(false);
        }

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

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            _GeneradorDeContrasenas = null;
            _DetectarOrden = null;
            _MostrarAutentificacionView = false;
            _MostrarDetectarServidoresLocalesView = false;
            _ObservadorAutentificacion = null;
            _ObservadorServidores = null;
        
            if (borrarCodigoAdministrado)
            {
                if (UsuarioNetzuela != null)
                {
                    UsuarioNetzuela.Dispose();
                    UsuarioNetzuela = null;
                }

                if (ContrasenaNetzuela != null)
                {
                    ContrasenaNetzuela.Dispose();
                    ContrasenaNetzuela = null;
                }

                if (_Autentificacion != null)
                {
                    _Autentificacion.Dispose();
                    _Autentificacion = null;
                }

                if (_Autentificacion != null)
                {
                    _ServidoresDetectados.Dispose();
                    _ServidoresDetectados = null;
                }

                if (_UsuarioExterno != null)
                {
                    _UsuarioExterno.Dispose();
                    _UsuarioExterno = null;
                }

                if (_ContrasenaExterna != null)
                {
                    _ContrasenaExterna.Dispose();
                    _ContrasenaExterna = null;
                }

                if (this._Conexion != null)
                {
                    this._Conexion.Dispose();
                    this._Conexion = null;
                }
            }
        }

        private void InicializarGeneradorDeContrasenas()
        {
            _GeneradorDeContrasenas = new PasswordGenerator();
            _GeneradorDeContrasenas.ConsecutiveCharacters = false;
            _GeneradorDeContrasenas.RepeatCharacters = false;
            _GeneradorDeContrasenas.Maximum = 20;
            _GeneradorDeContrasenas.Minimum = 18;
        }

        private void AbrirDetectarServidores()
        {
            ServidoresDetectados = new DetectarServidoresLocalesViewModel();

            _ObservadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.ServidoresDetectados)
                .RegisterHandler(n => n.MostrarView, this.CerrarDetectarServidores);

            MostrarDetectarServidoresLocalesView = true;
        }

        private void CerrarDetectarServidores(DetectarServidoresLocalesViewModel ServidoresVM)
        {
            try
            {
                if (ServidoresVM.MostrarView == false)
                {
                    MostrarDetectarServidoresLocalesView = false;
                    Parametros = ServidoresVM.Parametros.Clonar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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
            try
            {
                if (AutentificacionVM.MostrarView == false)
                {
                    MostrarAutentificacionView = false;

                    _UsuarioExterno = AutentificacionVM.Usuario.Copy();
                    _ContrasenaExterna = AutentificacionVM.Contrasena.Copy();
                                    
                    ConexionUsuario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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
            ContrasenaNetzuela = _GeneradorDeContrasenas.Generate().ConvertirASecureString();

            try
            {
                Resultado = this._Conexion.CrearUsuario(UsuarioNetzuela, ContrasenaNetzuela, ColumnasAutorizadas, Privilegios.Seleccionar);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario Netzuela", ex);
            }

            return Resultado;
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
