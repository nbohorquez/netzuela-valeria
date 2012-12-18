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
    using Zuliaworks.Netzuela.Valeria.Comunes;          // Asincronico
        
    /// <summary>
    /// 
    /// </summary>
    public partial class ClienteSpuria : Asincronico
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
        private string uriBaseServidor;
        private string uriServidorJsonSync;
        private CookieCollection cookies;

        #endregion

        #region Constructores

        public ClienteSpuria()
        {
            this.InicializarDelegados();
            this.aleatorio = new Random();
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

        protected override void InicializarDelegados()
        {
            this.delegadoDispararCambioEnProgresoDeOperacion = new SendOrPostCallback(AntesDeDispararCambioEnProgresoDeOperacion);
            this.delegadoDispararListarTiendasCompletado = new SendOrPostCallback(AntesDeDispararListarTiendasCompletado);
            this.delegadoDispararListarBDsCompletado = new SendOrPostCallback(AntesDeDispararListarBDsCompletado);
            this.delegadoDispararListarTablasCompletado = new SendOrPostCallback(AntesDeDispararListarTablasCompletado);
            this.delegadoDispararLeerTablaCompletado = new SendOrPostCallback(AntesDeDispararLeerTablaCompletado);
            this.delegadoDispararEscribirTablaCompletado = new SendOrPostCallback(AntesDeDispararEscribirTablaCompletado);
            this.delegadoDispararCrearUsuarioCompletado = new SendOrPostCallback(AntesDeDispararCrearUsuarioCompletado);
            this.delegadoDispararConsultarCompletado = new SendOrPostCallback(AntesDeDispararConsultarCompletado);
            base.InicializarDelegados();
        }

        #endregion

        #region Implementación de interfaces

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            this.UriBaseServicio = null;
            base.Dispose(borrarCodigoAdministrado);
        }
                
        #endregion
    }
}