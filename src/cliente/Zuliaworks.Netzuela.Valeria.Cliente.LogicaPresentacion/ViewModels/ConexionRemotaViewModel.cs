namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                  // DataTable, ConnectionState
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;
    using System.Windows;                               // MessageBox

    using MvvmFoundation.Wpf;                           // ObservableObject    
    using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    using Zuliaworks.Netzuela.Valeria.Cliente.Logica;   // Conexion

    /// <summary>
    /// 
    /// </summary>
    public class ConexionRemotaViewModel : ConexionViewModel, IDisposable
    {
        #region Variables

        private SeleccionarTiendaViewModel seleccionarTienda;
        private PropertyObserver<SeleccionarTiendaViewModel> observadorSeleccion;
        private string nombreTienda;
        private string resultadoEscribirTabla;
        private bool mostrarSeleccionarTiendaView;
        
        #endregion

        #region Constructores

        public ConexionRemotaViewModel()
            : base()
        {
        }

        public ConexionRemotaViewModel(Conexion conexion)
            : base(conexion)
        {
        }

        public ConexionRemotaViewModel(ParametrosDeConexion parametros)
            : base(parametros)
        {
        }

        ~ConexionRemotaViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Eventos

        public event EventHandler<EventoListarTiendasCompletadoArgs> ListarTiendasCompletado
        {
            add { this.conexion.ListarTiendasCompletado += value; }
            remove { this.conexion.ListarTiendasCompletado -= value; }
        }

        #endregion

        #region Propiedades

        public int TiendaId { get; set; }

        public string NombreTienda 
        { 
            get
            {
                return this.nombreTienda;
            }
            set
            {
                if (value != this.nombreTienda)
                {
                    this.nombreTienda = value;
                    this.RaisePropertyChanged("NombreTienda");
                }
            }
        }

        public SeleccionarTiendaViewModel SeleccionarTienda
        {
            get
            {
                return seleccionarTienda;
            }
            set
            {
                if (value != seleccionarTienda)
                {
                    seleccionarTienda = value;
                    this.RaisePropertyChanged("SeleccionarTienda");
                }
            }
        }

        public bool MostrarSeleccionarTiendaView
        {
            get
            {
                return mostrarSeleccionarTiendaView;
            }
            set
            {
                if (value != mostrarSeleccionarTiendaView)
                {
                    mostrarSeleccionarTiendaView = value;
                    this.RaisePropertyChanged("MostrarSeleccionarTiendaView");
                }
            }
        }

        public string ResultadoEscribirTabla
        {
            get
            {
                return this.resultadoEscribirTabla;
            }
            set
            {
                if (value != this.resultadoEscribirTabla)
                {
                    this.resultadoEscribirTabla = value;
                    this.RaisePropertyChanged("ResultadoEscribirTabla");
                }
            }
        }

        #endregion

        #region Funciones

        protected void ManejarEscribirTablaCompletado(object remitente, EventoEscribirTablaCompletadoArgs args)
        {
            try
            {
                this.conexion.EscribirTablaCompletado -= this.ManejarEscribirTablaCompletado;

                if (args.Error != null)
                {
                    this.ResultadoEscribirTabla = "Error: " + args.Error.Message;
                }
                else if (args.Cancelled)
                {
                    this.ResultadoEscribirTabla = "Cancelada";
                }
                else
                {
                    this.ResultadoEscribirTabla = args.Resultado == true ? "OK" : "No Ok";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected override void CerrarAutentificacion(AutentificacionViewModel AutentificacionVM)
        {
            try
            {
                if (AutentificacionVM.MostrarView == false)
                {
                    MostrarAutentificacionView = false;
                    Usuario = AutentificacionVM.Usuario.Copy();
                    Contrasena = AutentificacionVM.Contrasena.Copy();
                    AutentificacionVM.Dispose();
                    Autentificacion = null;
                    
                    this.ConexionOrdinaria();
                    this.AbrirSeleccionarTienda();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected void AbrirSeleccionarTienda()
        {
            if (SeleccionarTienda != null)
            {
                SeleccionarTienda.Dispose();
                SeleccionarTienda = null;
            }

            SeleccionarTienda = new SeleccionarTiendaViewModel(this);

            observadorSeleccion = new PropertyObserver<SeleccionarTiendaViewModel>(this.SeleccionarTienda)
                .RegisterHandler(n => n.MostrarView, this.CerrarSeleccionarTienda);

            MostrarSeleccionarTiendaView = true;
        }

        protected void CerrarSeleccionarTienda(SeleccionarTiendaViewModel seleccionarTiendaVM)
        {
            try
            {
                if (seleccionarTiendaVM.MostrarView == false)
                {
                    MostrarSeleccionarTiendaView = false;

                    this.TiendaId = seleccionarTiendaVM.Seleccion.Id;
                    this.NombreTienda = seleccionarTiendaVM.Seleccion.Nombre;

                    seleccionarTiendaVM.Dispose();
                    SeleccionarTienda = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            if (borrarCodigoAdministrado)
            {
                mostrarAutentificacionView = false;
                observadorAutentificacion = null;

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

                if (this.conexion != null)
                {
                    this.conexion.Dispose();
                    this.conexion = null;
                }

                if (this.autentificacion != null)
                {
                    this.autentificacion.Dispose();
                    this.autentificacion = null;
                }

                if (seleccionarTienda != null)
                {
                    seleccionarTienda.Dispose();
                    seleccionarTienda = null;
                }
            }
        }

        #region Métodos sincrónicos

        public string[] ListarTiendas()
        {
            string[] resultado = null;

            try
            {
                resultado = this.conexion.ListarTiendas();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        public override DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            DataTable resultado = null;

            try
            {
                resultado = this.conexion.LeerTabla(this.TiendaId, baseDeDatos, tabla);
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
                resultado = this.conexion.EscribirTabla(this.TiendaId, baseDeDatos, nombreTabla, tabla);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        #endregion
        
        #region Métodos asincrónicos

        public void ListarTiendasAsinc()
        {
            try
            {
                this.conexion.ListarTiendasAsinc();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
            try
            {
                this.conexion.LeerTablaAsinc(this.TiendaId, baseDeDatos, tabla);
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
                /* Esto esta horrible aqui... no se supone que deba registrar y desregistrar el manejador del evento
                 * cada vez que se llame a esta funcion */
                this.conexion.EscribirTablaCompletado -= this.ManejarEscribirTablaCompletado;
                this.conexion.EscribirTablaCompletado += this.ManejarEscribirTablaCompletado;
                this.conexion.EscribirTablaAsinc(this.TiendaId, baseDeDatos, nombreTabla, tabla);
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
