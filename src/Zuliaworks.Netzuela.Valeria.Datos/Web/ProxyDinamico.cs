namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                      // DataSet
    using System.Linq;
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

        private readonly BasicHttpBinding _Vinculacion;
        private readonly BasicHttpSecurity _Seguridad;
        private DynamicProxyFactory _Fabrica;
        private DynamicProxy _ProxyDinamico;
        private string _UriWsdlServicio;
        private bool _UriWsdlServicioModificado;

        #endregion

        #region Constructores

        public ProxyDinamico() { }

        public ProxyDinamico(string UriWsdlServicio)
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");

            this.UriWsdlServicio = UriWsdlServicio;
            this._UriWsdlServicioModificado = false;
            CrearFabrica();            
            
            _Seguridad = new BasicHttpSecurity()
            {
                Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                Transport = { 
                    ClientCredentialType = HttpClientCredentialType.Basic
                }
            };
            
            _Vinculacion = new BasicHttpBinding()
            {
                Name = "ServidorApi",
                Namespace = Constantes.Namespace,
                Security = _Seguridad
            };
        }

        ~ProxyDinamico()
        {
            Dispose(false);
        }

        #endregion

        #region Propiedades

        public string UriWsdlServicio 
        {
            get { return _UriWsdlServicio; }
            set
            {
                if (value != _UriWsdlServicio)
                {
                    if (_ProxyDinamico == null)
                    {
                        _UriWsdlServicio = value;
                        _UriWsdlServicioModificado = true;
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

        private void CrearFabrica()
        {
            // En este instruccion es donde se consume la mayor cantidad de tiempo de ejecucion
            _Fabrica = new DynamicProxyFactory(UriWsdlServicio);
        }

        private void Abortar()
        {
            _ProxyDinamico.CallMethod("Abort");
        }

        public void Conectar(string Contrato)
        {
            Desconectar();

            if (Contrato == null)
            {
                throw new ArgumentNullException("Contrato");
            }

            try
            {
                if (_UriWsdlServicioModificado)
                {
                    _UriWsdlServicioModificado = false;
                    
                    if (UriWsdlServicio == null)
                    {
                        throw new ArgumentNullException("UriWsdlServicio");
                    }

                    CrearFabrica();
                }

                ServiceEndpoint Endpoint = null;

                foreach (ServiceEndpoint SE in _Fabrica.Endpoints)
                {
                    if (SE.Contract.Name.Contains(Contrato))
                    {
                        Endpoint = SE;
                        break;
                    }
                }

                // Al usar WSHttpBinding se pierde la capacidad de enviar los datos en un flujo sin fin 
                // (streaming). Por lo que se hace obligatorio el uso de intermedios (buffers) para enviar
                // archivos o datos extensos.
                // http://kjellsj.blogspot.com/2007/02/wcf-streaming-upload-files-over-http.html

                // Las configuraciones de "binding" del servidor no son visibles del lado del cliente a traves de WSDL,
                // segun http://social.msdn.microsoft.com/Forums/en-AU/wcf/thread/dcc46d86-87a5-4694-aa88-3568fddf159f.
                // Es necesario crear un nuevo binding del lado del cliente con las opciones de configuracion deseadas.

                Endpoint.Binding = _Vinculacion;
                _ProxyDinamico = _Fabrica.CreateProxy(Contrato);
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando ProxyDinamico", ex);
            }
        }

        public object InvocarMetodo(string Metodo, params object[] Argumentos)
        {
            object Resultado = null;

            // Si alguno de los argumentos es un DataContract entonces hay que convertirlo en DynamicObject
            if (Argumentos != null)
            {
                for (int i = 0; i < Argumentos.Length; i++)
                {
                    if (Argumentos[i] is DataTableXml)
                    {
                        Argumentos[i] = ((DataTableXml)Argumentos[i]).ConvertirEnObjetoDinamico(_Fabrica.ProxyAssembly);
                    }
                }
            }

            try
            {
                var Credenciales = (ClientCredentials)_ProxyDinamico.GetProperty("ClientCredentials");
                //Credenciales.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                Credenciales.UserName.UserName = "molleja@abc.com";
                Credenciales.UserName.Password = "41ssdas#ASX";
                
                // Con codigo de: http://stackoverflow.com/questions/1544830/wcf-transportcredentialonly-not-sending-username-and-password
                using (OperationContextScope scope = new OperationContextScope((IClientChannel)_ProxyDinamico.GetProperty("InnerChannel")))
                {
                    var propiedadPeticionHttp = new HttpRequestMessageProperty();
                    string autorizacion = (Credenciales.UserName.UserName + ":" + Credenciales.UserName.Password).CodificarBase64();
                    propiedadPeticionHttp.Headers[System.Net.HttpRequestHeader.Authorization] = "Basic " + autorizacion;
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = propiedadPeticionHttp;
                  
                    Resultado = _ProxyDinamico.CallMethod(Metodo, Argumentos);
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
                if (Resultado.GetType().Name == typeof(DataTableXml).Name)
                {
                    Resultado = new DataTableXml(new DataTableXmlDinamico(Resultado));
                }
            }
            catch (Exception ex)
            {
                Abortar();
                throw new Exception("Error invocando el metodo \"" + Metodo + "\" en el servidor remoto", ex);
            }

            return Resultado;
        }

        public void Desconectar()
        {
            try
            {
                if (_ProxyDinamico != null)
                {
                    _ProxyDinamico.Close();
                    _ProxyDinamico = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error desconectando a ProxyDinamico", ex);
            }
        }

        #endregion

        #region Implementacion de interfaces

        protected override void Dispose(bool BorrarCodigoAdministrado)
        {
            this._Fabrica = null;
            this.Desconectar();

            if (BorrarCodigoAdministrado)
            {
            }
        }

        #endregion
    }
}
