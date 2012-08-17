namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                      // DataSet
    using System.Linq;
    using System.Security;                                  // SecureString
    using System.Security.Cryptography.X509Certificates;    // StoreLocation, StoreName, X509FindType
    using System.ServiceModel;                              // WSHttpBinding
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;                  // ServiceEndpoint
    using System.ServiceModel.Dispatcher;                   // ClienRuntime
    using System.ServiceModel.Security;                     // X509CertificateValidationMode
    using System.Text;

    using WcfSamples.DynamicProxy;                          // DynamicProxyFactory
    
    using Zuliaworks.Netzuela.Spuria.TiposApi;              // DataSetXml
    using Zuliaworks.Netzuela.Valeria.Comunes;

    public class ProxyDinamico : Desechable
    {
        /*
         * Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx
         */

        #region Variables

        private readonly BasicHttpBinding vinculacion;
        private readonly BasicHttpSecurity seguridad;
        private DynamicProxyFactory fabrica;
        private DynamicProxy proxyDinamico;
        private string uriWsdlServicio;
        private bool uriWsdlServicioModificado;
        private SecureString usuario;
        private SecureString contrasena;

        #endregion

        #region Constructores

        public ProxyDinamico() 
        { 
        }

        public ProxyDinamico(string uriWsdlServicio)
        {
            if (uriWsdlServicio == null)
            {
                throw new ArgumentNullException("UriWsdlServicio");
            }

            this.UriWsdlServicio = uriWsdlServicio;
            this.uriWsdlServicioModificado = false;
            this.CrearFabrica();            
            
            this.seguridad = new BasicHttpSecurity()
            {
                Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                Transport = 
                { 
                    ClientCredentialType = HttpClientCredentialType.Basic
                }
            };
            
            this.vinculacion = new BasicHttpBinding()
            {
                Name = "ServidorApi",
                Namespace = Constantes.Namespace,
                Security = this.seguridad
            };
        }

        public ProxyDinamico(string uriWsdlServicio, SecureString usuario, SecureString contrasena)
            : this(uriWsdlServicio)
        {
            this.usuario = usuario;
            this.contrasena = contrasena;
        }

        ~ProxyDinamico()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public string UriWsdlServicio 
        {
            get 
            { 
                return this.uriWsdlServicio; 
            }

            set
            {
                if (value != this.uriWsdlServicio)
                {
                    if (this.proxyDinamico == null)
                    {
                        this.uriWsdlServicio = value;
                        this.uriWsdlServicioModificado = true;
                    }
                    else
                    {
                        throw new FieldAccessException("No se puede modificar UriWsdlServicio mientras la conexión este establecida");
                    }
                }
            }
        }

        #endregion

        #region Funciones

        public void Conectar(string Contrato)
        {
            this.Desconectar();

            if (Contrato == null)
            {
                throw new ArgumentNullException("Contrato");
            }

            try
            {
                if (this.uriWsdlServicioModificado)
                {
                    this.uriWsdlServicioModificado = false;

                    if (this.UriWsdlServicio == null)
                    {
                        throw new ArgumentNullException("UriWsdlServicio");
                    }

                    this.CrearFabrica();
                }

                ServiceEndpoint Endpoint = null;

                foreach (ServiceEndpoint SE in this.fabrica.Endpoints)
                {
                    if (SE.Contract.Name.Contains(Contrato))
                    {
                        Endpoint = SE;
                        break;
                    }
                }

                /*
                 * Al usar WSHttpBinding se pierde la capacidad de enviar los datos en un flujo sin fin 
                 * (streaming). Por lo que se hace obligatorio el uso de intermedios (buffers) para enviar
                 * archivos o datos extensos.
                 * http://kjellsj.blogspot.com/2007/02/wcf-streaming-upload-files-over-http.html
                 * 
                 * Las configuraciones de "binding" del servidor no son visibles del lado del cliente a traves de WSDL,
                 * segun http://social.msdn.microsoft.com/Forums/en-AU/wcf/thread/dcc46d86-87a5-4694-aa88-3568fddf159f.
                 * Es necesario crear un nuevo binding del lado del cliente con las opciones de configuracion deseadas.
                 */

                Endpoint.Binding = this.vinculacion;
                this.proxyDinamico = this.fabrica.CreateProxy(Contrato);
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando ProxyDinamico", ex);
            }
        }

        public object InvocarMetodo(string metodo, params object[] argumentos)
        {
            object resultado = null;

            // Si alguno de los argumentos es un DataContract entonces hay que convertirlo en DynamicObject
            if (argumentos != null)
            {
                for (int i = 0; i < argumentos.Length; i++)
                {
                    if (argumentos[i] is DataTableXml)
                    {
                        argumentos[i] = ((DataTableXml)argumentos[i]).ConvertirEnObjetoDinamico(this.fabrica.ProxyAssembly);
                    }
                }
            }

            try
            {
                var credenciales = (ClientCredentials)this.proxyDinamico.GetProperty("ClientCredentials");
                //Credenciales.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                credenciales.UserName.UserName = this.usuario.ConvertirAUnsecureString();
                credenciales.UserName.Password = this.contrasena.ConvertirAUnsecureString();

                // Con codigo de: http://stackoverflow.com/questions/1544830/wcf-transportcredentialonly-not-sending-username-and-password
                using (OperationContextScope scope = new OperationContextScope((IClientChannel)this.proxyDinamico.GetProperty("InnerChannel")))
                {
                    if (this.usuario != null && this.contrasena != null)
                    {
                        var propiedadPeticionHttp = new HttpRequestMessageProperty();
                        string autorizacion = (this.usuario.ConvertirAUnsecureString() + ":" + this.contrasena.ConvertirAUnsecureString()).CodificarBase64();
                        propiedadPeticionHttp.Headers[System.Net.HttpRequestHeader.Authorization] = "Basic " + autorizacion;
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = propiedadPeticionHttp;
                    }

                    resultado = this.proxyDinamico.CallMethod(metodo, argumentos);
                }

                /*
                 * No puedo hacer "if(Resultado is DataSetXML)" por que ocurre este error:
                 * http://stackoverflow.com/questions/2500280/invalidcastexception-for-two-objects-of-the-same-type
                 * 
                 * Anteriormente la comparacion era: (Resultado.GetType().FullName == typeof(DataTableXml).FullName)
                 * porque los tipos involucrados eran ambos 'Zuliaworks.Netzuela.Spuria.TiposApi.DataTableXml'. Sin
                 * embargo, luego de que pase a Mono (y probablemente luego de cambiar muchos Namespace en la confi-
                 * guracion del servidor a http://netzuela.zuliaworks.com/spuria/api) el tipo al que pertenece 
                 * Resultado es ahora 'netzuela.zuliaworks.com.spuria.api.DataTableXml' por lo que ya no puedo hacer 
                 * la misma comparacion FullName sino solo Name.
                 */
                if (resultado.GetType().Name == typeof(DataTableXml).Name)
                {
                    resultado = new DataTableXml(new DataTableXmlDinamico(resultado));
                }
            }
            catch (Exception ex)
            {
                this.Abortar();
                throw new Exception("Error invocando el metodo \"" + metodo + "\" en el servidor remoto", ex);
            }

            return resultado;
        }

        public void Desconectar()
        {
            try
            {
                if (this.proxyDinamico != null)
                {
                    this.proxyDinamico.Close();
                    this.proxyDinamico = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error desconectando a ProxyDinamico", ex);
            }
        }

        private void CrearFabrica()
        {
            // En este instruccion es donde se consume la mayor cantidad de tiempo de ejecucion
            this.fabrica = new DynamicProxyFactory(this.UriWsdlServicio);
        }

        private void Abortar()
        {
            this.proxyDinamico.CallMethod("Abort");
        }

        #endregion

        #region Implementacion de interfaces

        protected override void Dispose(bool BorrarCodigoAdministrado)
        {
            this.uriWsdlServicio = null;
            this.uriWsdlServicioModificado = false;
            this.fabrica = null;
            this.Desconectar();

            if (BorrarCodigoAdministrado)
            {
                if (usuario != null)
                {
                    usuario.Dispose();
                    usuario = null;
                }

                if (contrasena != null)
                {
                    contrasena.Dispose();
                    contrasena = null;
                }
            }
        }

        #endregion
    }
}
