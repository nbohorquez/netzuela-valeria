namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                          // DataTable, ConnectionState
    using System.Linq;
    using System.Security;                                      // SecureString
    using System.Text;
    using System.Windows;                                       // MessageBox
    using System.Windows.Input;                                 // ICommand

    using MvvmFoundation.Wpf;                                   // RelayCommand, ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;                  // DatosDeConexion, PasswordGenerator
    using Zuliaworks.Netzuela.Valeria.Datos;                    // Conexion

    /// <summary>
    /// 
    /// </summary>
    public class ConexionLocalViewModel : ConexionViewModel, IDisposable
    {
        #region Variables

        private PasswordGenerator generadorDeContrasenas;
        private RelayCommand detectarOrden;
        private bool mostrarDetectarServidoresLocalesView;
        private PropertyObserver<DetectarServidoresLocalesViewModel> observadorServidores;
        private DetectarServidoresLocalesViewModel servidoresDetectados;
        private SecureString usuarioProtegido;
        private SecureString contrasenaProtegida;
        private const string UsuarioOrdinario = "netzuela";

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

        public DetectarServidoresLocalesViewModel ServidoresDetectados
        {
            get 
            { 
                return servidoresDetectados; 
            }
            set
            {
                if (value != servidoresDetectados)
                {
                    servidoresDetectados = value;
                    this.RaisePropertyChanged("ServidoresDetectados");
                }
            }
        }

        public bool MostrarDetectarServidoresLocalesView
        {
            get 
            { 
                return mostrarDetectarServidoresLocalesView; 
            }
            set
            {
                if (value != mostrarDetectarServidoresLocalesView)
                {
                    mostrarDetectarServidoresLocalesView = value;
                    this.RaisePropertyChanged("MostrarDetectarServidoresLocalesView");
                }
            }
        }

        public ICommand DetectarOrden
        {
            get { return detectarOrden ?? (detectarOrden = new RelayCommand(this.AbrirDetectarServidores)); }
        }

        #endregion

        #region Funciones

        private void InicializarGeneradorDeContrasenas()
        {
            generadorDeContrasenas = new PasswordGenerator();
            generadorDeContrasenas.ConsecutiveCharacters = false;
            generadorDeContrasenas.RepeatCharacters = false;
            generadorDeContrasenas.Maximum = 20;
            generadorDeContrasenas.Minimum = 18;
        }

        private void AbrirDetectarServidores()
        {
            if (ServidoresDetectados != null)
            {
                ServidoresDetectados.Dispose();
                ServidoresDetectados = null;
            }

            ServidoresDetectados = new DetectarServidoresLocalesViewModel();

            observadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.ServidoresDetectados)
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
                    ServidoresVM.Dispose();
                    ServidoresDetectados = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ConexionProtegida()
        {
            this.Conectar(usuarioProtegido, contrasenaProtegida);

            // Borramos todo aquello que pudiese resultar atractivo para alguien con malas intenciones
            if (usuarioProtegido != null)
            {
                usuarioProtegido.Dispose();
                usuarioProtegido = null;
            }
            if (contrasenaProtegida != null)
            {
                contrasenaProtegida.Dispose();
                contrasenaProtegida = null;
            }
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            generadorDeContrasenas = null;
            detectarOrden = null;
            mostrarAutentificacionView = false;
            mostrarDetectarServidoresLocalesView = false;
            observadorAutentificacion = null;
            observadorServidores = null;

            if (borrarCodigoAdministrado)
            {
                if (Usuario != null)
                {
                    Usuario.Dispose();
                    Usuario = null;
                }

                if (Contrasena != null)
                {
                    Contrasena.Dispose();
                    Contrasena = null;
                }

                if (autentificacion != null)
                {
                    autentificacion.Dispose();
                    autentificacion = null;
                }

                if (servidoresDetectados != null)
                {
                    servidoresDetectados.Dispose();
                    servidoresDetectados = null;
                }

                if (usuarioProtegido != null)
                {
                    usuarioProtegido.Dispose();
                    usuarioProtegido = null;
                }

                if (contrasenaProtegida != null)
                {
                    contrasenaProtegida.Dispose();
                    contrasenaProtegida = null;
                }

                if (this.conexion != null)
                {
                    this.conexion.Dispose();
                    this.conexion = null;
                }
            }
        }

        protected override void CerrarAutentificacion(AutentificacionViewModel AutentificacionVM)
        {
            try
            {
                if (AutentificacionVM.MostrarView == false)
                {
                    MostrarAutentificacionView = false;
                    usuarioProtegido = AutentificacionVM.Usuario.Copy();
                    contrasenaProtegida = AutentificacionVM.Contrasena.Copy();
                    AutentificacionVM.Dispose();
                    Autentificacion = null;
                    ConexionProtegida();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        public bool CrearUsuarioOrdinario(string[] ColumnasAutorizadas)
        {
            bool Resultado = false;
            // ¿Será un problema de seguridad grave colocar el nombre "netzuela" asi tan a la vista?
            Usuario = ConexionLocalViewModel.UsuarioOrdinario.ConvertirASecureString();
            Contrasena = generadorDeContrasenas.Generate().ConvertirASecureString();

            try
            {
                Resultado = this.conexion.CrearUsuario(Usuario, Contrasena, ColumnasAutorizadas, Privilegios.Seleccionar);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario ordinario", ex);
            }

            return Resultado;
        }

        #region Métodos sincrónicos

        public override DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            DataTable resultado = null;

            try
            {
                resultado = this.conexion.LeerTabla(baseDeDatos, tabla);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        public override bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            bool resultado = false;

            try
            {
                resultado = this.conexion.EscribirTabla(baseDeDatos, nombreTabla, tabla);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        #endregion

        #region Métodos asincrónicos

        public override void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
            try
            {
                this.conexion.LeerTablaAsinc(baseDeDatos, tabla);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            try
            {
                this.conexion.EscribirTablaAsinc(baseDeDatos, nombreTabla, tabla);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

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
