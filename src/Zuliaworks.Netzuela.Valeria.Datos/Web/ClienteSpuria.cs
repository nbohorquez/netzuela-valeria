namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;               // HybridDictionary
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;
    using System.Threading;                             // Thread, SendOrPostCallback

    using Zuliaworks.Netzuela.Valeria.Comunes;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class ClienteSpuria : Desechable
    {
        /*
         * Referencia 1: http://msdn.microsoft.com/en-us/library/bz33kx67.aspx
         * Referencia 2: http://msdn.microsoft.com/en-us/library/wewwczdw.aspx
         */
        
        #region Variables y Constantes

        private const string ContratoSpuria = "IApi";
        private Random aleatorio;
        private ProxyDinamico proxy;
        private HybridDictionary hilos;
        private string uriWsdlServicio;

        #endregion

        #region Constructores

        public ClienteSpuria()
        {
            this.InicializarDelegados();
            this.aleatorio = new Random();
            this.hilos = new HybridDictionary();
        }

        public ClienteSpuria(string uriWsdlServicio)
            : this()
        {
            if (uriWsdlServicio == null)
            {
                throw new ArgumentNullException("uriWsdlServicio");
            }

            this.UriWsdlServicio = uriWsdlServicio;
        }

        ~ClienteSpuria()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public string UriWsdlServicio
        {
            get 
            { 
                return (this.proxy == null) ? this.uriWsdlServicio : this.proxy.UriWsdlServicio; 
            }

            set
            {
                if (this.proxy == null)
                {
                    this.uriWsdlServicio = value;
                }
                else
                {
                    this.proxy.UriWsdlServicio = value;
                }
            }
        }

        #endregion

        #region Funciones

        public void Armar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                this.proxy = new ProxyDinamico(this.UriWsdlServicio, usuario, contrasena);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando ProxyDinamico con argumento: \"" + this.UriWsdlServicio + "\"", ex);
            }
        }

        public void Desarmar()
        {
            try
            {
                if (this.proxy != null)
                {
                    this.proxy.Dispose();
                    this.proxy = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando ProxyDinamico con argumento: \"" + this.UriWsdlServicio + "\"", ex);
            }
        }

        public void Conectar()
        {
            try
            {
                this.proxy.Conectar(ContratoSpuria);
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando con servidor remoto Spuria", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                this.proxy.Desconectar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error desconectando de servidor remoto Spuria", ex);
            }
        }

        public void CancelarTarea(object TareaID)
        {
            try
            {
                AsyncOperation asincronico = (AsyncOperation)this.hilos[TareaID];

                if (asincronico != null)
                {
                    lock (this.hilos.SyncRoot)
                    {
                        this.hilos.Remove(TareaID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelando la tarea " + TareaID.ToString(), ex);
            }
        }

        private bool TareaCancelada(object tareaId)
        {
            return this.hilos[tareaId] == null;
        }

        protected virtual void InicializarDelegados()
        {
            this.delegadoDispararCambioEnProgresoDeOperacion = new SendOrPostCallback(AntesDeDispararCambioEnProgresoDeOperacion);
            this.delegadoDispararListarTiendasCompletado = new SendOrPostCallback(AntesDeDispararListarTiendasCompletado);
            this.delegadoDispararListarBDsCompletado = new SendOrPostCallback(AntesDeDispararListarBDsCompletado);
            this.delegadoDispararListarTablasCompletado = new SendOrPostCallback(AntesDeDispararListarTablasCompletado);
            this.delegadoDispararLeerTablaCompletado = new SendOrPostCallback(AntesDeDispararLeerTablaCompletado);
            this.delegadoDispararEscribirTablaCompletado = new SendOrPostCallback(AntesDeDispararEscribirTablaCompletado);
            this.delegadoDispararCrearUsuarioCompletado = new SendOrPostCallback(AntesDeDispararCrearUsuarioCompletado);
            this.delegadoDispararConsultarCompletado = new SendOrPostCallback(AntesDeDispararConsultarCompletado);
            this.carpintero = new DelegadoComenzarOperacion(ComenzarOperacion);
        }

        #endregion

        #region Implementación de interfaces

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            this.UriWsdlServicio = null;

            if (borrarCodigoAdministrado)
            {
                if (this.hilos != null)
                {
                    foreach (var entrada in this.hilos.Keys)
                    {
                        this.CancelarTarea(entrada);
                    }

                    this.hilos.Clear();
                    this.hilos = null;
                }

                if (this.proxy != null)
                {
                    this.proxy.Dispose();
                    this.proxy = null;
                }
            }
        }
                
        #endregion
    }
}
