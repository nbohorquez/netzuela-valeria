namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                  // DataTable, ConnectionState
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;
    using System.Windows;                               // MessageBox
    using System.Windows.Input;                         // ICommand

    using MvvmFoundation.Wpf;                           // ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
    using Zuliaworks.Netzuela.Valeria.Datos;            // Conexion
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;

    public class ConexionViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand conectarDesconectarOrden;
        private bool permitirModificaciones;
        protected bool mostrarAutentificacionView;
        protected AutentificacionViewModel autentificacion;
        protected PropertyObserver<AutentificacionViewModel> observadorAutentificacion;
        protected Conexion conexion;
        
        #endregion

        #region Constructores

        public ConexionViewModel()
        {
            this.conexion = new Conexion();
        }

        public ConexionViewModel(Conexion Conexion)
        {
            if (Conexion == null)
            {
                throw new ArgumentNullException("Conexion");
            }

            this.conexion = Conexion;
        }

        public ConexionViewModel(ParametrosDeConexion Parametros)
        {
            if (Parametros == null)
            {
                throw new ArgumentNullException("Parametros");
            }

            this.conexion = new Conexion(Parametros);
        }

        #endregion

        #region Eventos

        public event StateChangeEventHandler CambioDeEstado
        {
            add { this.conexion.CambioDeEstado += value; }
            remove { this.conexion.CambioDeEstado -= value; }
        }

        public event EventHandler<EventoListarBDsCompletadoArgs> ListarBasesDeDatosCompletado
        {
            add { this.conexion.ListarBasesDeDatosCompletado += value; }
            remove { this.conexion.ListarBasesDeDatosCompletado -= value; }
        }

        public event EventHandler<EventoListarTablasCompletadoArgs> ListarTablasCompletado
        {
            add { this.conexion.ListarTablasCompletado += value; }
            remove { this.conexion.ListarTablasCompletado -= value; }
        }

        public event EventHandler<EventoLeerTablaCompletadoArgs> LeerTablaCompletado
        {
            add { this.conexion.LeerTablaCompletado += value; }
            remove { this.conexion.LeerTablaCompletado -= value; }
        }

        public event EventHandler<EventoEscribirTablaCompletadoArgs> EscribirTablaCompletado
        {
            add { this.conexion.EscribirTablaCompletado += value; }
            remove { this.conexion.EscribirTablaCompletado -= value; }
        }

        public event EventHandler<EventoCrearUsuarioCompletadoArgs> CrearUsuarioCompletado
        {
            add { this.conexion.CrearUsuarioCompletado += value; }
            remove { this.conexion.CrearUsuarioCompletado -= value; }
        }

        public event EventHandler<EventoConsultarCompletadoArgs> ConsultarCompletado
        {
            add { this.conexion.ConsultarCompletado += value; }
            remove { this.conexion.ConsultarCompletado -= value; }
        }

        #endregion

        #region Propiedades

        public SecureString Usuario { get; protected set; }
        public SecureString Contrasena { get; protected set; }
        /*
        public Conexion Conexion
        {
            get { return this.conexion; }
        }
        */
        public ConnectionState Estado
        {
            get { return this.conexion.Estado; }
        }

        public AutentificacionViewModel Autentificacion
        {
            get
            {
                return autentificacion;
            }
            protected set
            {
                if (value != autentificacion)
                {
                    autentificacion = value;
                    this.RaisePropertyChanged("Autentificacion");
                }
            }
        }

        public bool MostrarAutentificacionView
        {
            get
            {
                return mostrarAutentificacionView;
            }
            set
            {
                if (value != mostrarAutentificacionView)
                {
                    mostrarAutentificacionView = value;
                    this.RaisePropertyChanged("MostrarAutentificacionView");
                }
            }
        }

        public ParametrosDeConexion Parametros
        {
            get 
            { 
                return this.conexion.Parametros; 
            }
            set
            {
                if (value != this.conexion.Parametros)
                {
                    this.conexion.Parametros = value;
                    this.RaisePropertyChanged("Parametros");
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
            get { return conectarDesconectarOrden ?? (conectarDesconectarOrden = new RelayCommand(this.ConectarDesconectarAccion)); }
        }

        #endregion

        #region Funciones
        
        private void ConectarDesconectarAccion()
        {
            try
            {
                this.ConectarDesconectar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected void AbrirAutentificacion()
        {
            if (this.Autentificacion != null)
            {
                this.Autentificacion.Dispose();
                this.Autentificacion = null;
            }

            this.Autentificacion = new AutentificacionViewModel();

            observadorAutentificacion = new PropertyObserver<AutentificacionViewModel>(this.Autentificacion)
                .RegisterHandler(n => n.MostrarView, this.CerrarAutentificacion);

            this.MostrarAutentificacionView = true;
        }

        protected virtual void CerrarAutentificacion(AutentificacionViewModel AutentificacionVM)
        {
            try
            {
                if (AutentificacionVM.MostrarView == false)
                {
                    this.MostrarAutentificacionView = false;
                    this.Usuario = AutentificacionVM.Usuario.Copy();
                    this.Contrasena = AutentificacionVM.Contrasena.Copy();
                    AutentificacionVM.Dispose();
                    this.Autentificacion = null;
                    this.ConexionOrdinaria();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected virtual void ConectarDesconectar()
        {
            if (this.Estado == ConnectionState.Open)
            {
                this.Desconectar();
            }
            else
            {
                this.AbrirAutentificacion();
            }
        }

        public void ConexionOrdinaria()
        {
            try
            {
                this.Conectar(Usuario, Contrasena);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al tratar de establecer la conexión local con el usuario ordinario", ex);
            }
        }

        public void ManejarCambioDeEstado(object remitente, StateChangeEventArgs args)
        {
            this.RaisePropertyChanged("Estado");                     // Para la gente de MainViewModel
            this.RaisePropertyChanged("EstadoString");               // Para la gente de BarraDeEstadoView
            this.RaisePropertyChanged("BotonConectarDesconectar");   // Para la gente de ConexionLocalView y ConexionRemotaView
            this.RaisePropertyChanged("PermitirModificaciones");     // Para la gente de ConexionLocalView y ConexionRemotaView
        }

        public void ResolverDatosDeConexion()
        {
            try
            {
                this.conexion.ResolverDatosDeConexion();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al tratar de resolver los datos de conexión", ex);
            }
        }

        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            this.ResolverDatosDeConexion();
            
            try
            {
                // Esto es para evitar que aparezca registrado el mismo manejador dos veces
                this.CambioDeEstado -= this.ManejarCambioDeEstado;
                this.CambioDeEstado += this.ManejarCambioDeEstado;
                this.conexion.Conectar(Usuario, Contrasena);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Desconectar()
        {
            try
            {
                this.conexion.Desconectar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string[] ListarBasesDeDatos()
        {
            string[] resultado = null;

            try
            {
                resultado = this.conexion.ListarBasesDeDatos();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            string[] resultado = null;

            try
            {
                resultado = this.conexion.ListarTablas(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        public virtual DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            return new DataTable();
        }

        public virtual bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            return false;
        }

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnasAutorizadas, int privilegios)
        {
            bool resultado = false;

            try
            {
                resultado = this.conexion.CrearUsuario(usuario, contrasena, columnasAutorizadas, Privilegios.Seleccionar);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            DataTable resultado = null;

            try
            {
                resultado = this.conexion.Consultar(baseDeDatos, sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            try
            {
                this.conexion.ListarBasesDeDatosAsinc();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ListarTablasAsinc(string baseDeDatos)
        {
            try
            {
                this.conexion.ListarTablasAsinc(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
        }

        public virtual void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            try
            {
                this.conexion.CrearUsuarioAsinc(usuario, contrasena, columnas, privilegios);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConsultarAsinc(string baseDeDatos, string sql)
        {
            try
            {
                this.conexion.ConsultarAsinc(baseDeDatos, sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion
    }
}
