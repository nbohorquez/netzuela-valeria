namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Specialized;               // HybridDictionary
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.IO;
    using System.Net;
    using System.Security;                              // SecureString
    using System.Threading;                             // Thread, SendOrPostCallback

    using ServiceStack.Service;                         // IServiceClient
    using ServiceStack.ServiceClient.Web;               // JsonServiceClient
    using ServiceStack.ServiceModel.Serialization;      // JsonDataContractDeserializer
    using Zuliaworks.Netzuela.Valeria.Tipos;
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

        private const string JSON = "/json";
        private const string XML = "/xml";
        private const string HTML = "/html";
        private const string JSV = "/jsv";
        private const string CSV = "/csv";
        private const string SOAP11 = "/soap11/";
        private const string SOAP12 = "/soap12/";
        private const string SYNC = "/syncreply/";
        private const string ASYNC = "/asynconeway/";

        private IServiceClient cliente;
        private Random aleatorio;
        private HybridDictionary hilos;
        private string uriBaseServidor;
        private string uriServidorJsonSync;
        private CookieCollection cookies;

        #endregion

        #region Constructores

        public ClienteSpuria()
        {
            this.InicializarDelegados();
            this.aleatorio = new Random();
            this.hilos = new HybridDictionary();
            this.cookies = new CookieCollection();
        }

        public ClienteSpuria(string uriBaseServicio)
            : this()
        {
            if (uriBaseServicio == null)
            {
                throw new ArgumentNullException("uriBaseServicio");
            }

            this.UriBaseServicio = uriBaseServicio;
        }

        ~ClienteSpuria()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public string UriBaseServicio
        {
            get 
            { 
                return this.uriBaseServidor; 
            }
            set
            {
                this.uriBaseServidor = value;
                this.uriServidorJsonSync = value + JSON + SYNC;
            }
        }

        #endregion

        #region Funciones

        public void Conectar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                using (cliente = new JsonServiceClient(this.UriBaseServicio))
                {
                    var peticion = new Auth()
                    {
                        UserName = usuario.ConvertirAUnsecureString(),
                        Password = contrasena.ConvertirAUnsecureString(),
                        RememberMe = true
                    };                    
                    var respuesta = cliente.Send<AuthResponse>(peticion);
                    if (respuesta.ResponseStatus.ErrorCode != null)
                    {
                        throw new Exception(respuesta.ResponseStatus.Message);
                    }

                    this.cookies.Add(new Cookie("ss-id", respuesta.SessionId, "/", this.UriBaseServicio));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando con servidor remoto", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                this.cookies = new CookieCollection();
            }
            catch (Exception ex)
            {
                throw new Exception("Error desconectando de servidor remoto", ex);
            }
        }

        public void CancelarTarea(object tareaId)
        {
            try
            {
                AsyncOperation asincronico = (AsyncOperation)this.hilos[tareaId];

                if (asincronico != null)
                {
                    lock (this.hilos.SyncRoot)
                    {
                        this.hilos.Remove(tareaId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelando la tarea " + tareaId.ToString(), ex);
            }
        }

        private bool TareaCancelada(object tareaId)
        {
            return this.hilos[tareaId] == null;
        }

        private void AgregarCookies(HttpWebRequest peticion)
        {
            peticion.CookieContainer = new CookieContainer();

            for (int i = 0, length = this.cookies.Count; i < length; i++)
            {
                Cookie cookie = new Cookie(this.cookies[i].Name, this.cookies[i].Value, this.cookies[i].Path);
                peticion.CookieContainer.Add(peticion.RequestUri, cookie);
            }
        }

        private T ObtenerRespuesta<T>(WebRequest peticion)
        {
            var respuestaJson = new StreamReader(peticion.GetResponse().GetResponseStream()).ReadToEnd();
            return (T)JsonDataContractDeserializer.Instance.DeserializeFromString(respuestaJson, typeof(T));
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
            this.UriBaseServicio = null;

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
            }
        }
                
        #endregion
    }
}