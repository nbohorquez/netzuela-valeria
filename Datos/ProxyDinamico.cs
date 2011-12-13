using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;              // DynamicProxyFactory
using System.Data;                          // DataSet
using System.ServiceModel;                  // WSHttpBinding
using System.ServiceModel.Description;      // ServiceEndpoint

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    [Serializable]
    public class ProxyDinamico : IDisposable
    {
        // Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx

        #region Variable

        private string _UriWsdlServicio;
        private DynamicProxyFactory _Fabrica;
        private DynamicProxy _IValeria;

        #endregion

        #region Constructores
        
        public ProxyDinamico(string UriWsdlServicio)
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");
                
            _UriWsdlServicio = UriWsdlServicio;
        }

        ~ProxyDinamico()
        {
            Dispose(false);
        }

        #endregion
        
        #region Funciones

        private void Dispose(bool BorrarCodigoAdministrado)
        {
            this.Desconectar();

            if (BorrarCodigoAdministrado) { }
        }
        private void CrearProxy()
        {
            ServiceEndpoint Endpoint = null;

            // En este instruccion es donde se consume la mayor cantidad de tiempo de ejecucion
            _Fabrica = new DynamicProxyFactory(_UriWsdlServicio);

            foreach (ServiceEndpoint SE in _Fabrica.Endpoints)
            {
                if (SE.Contract.Name.Contains("IValeria"))
                {
                    Endpoint = SE;
                }
            }

            if (Endpoint != null)
            {
                // Al usar WsHttpBinding se pierde la capacidad de enviar los datos en un flujo sin fin 
                // (streaming). Por lo que se hace obligatorio el uso de intermedios (buffers) para enviar
                // archivos o datos extensos.
                // http://kjellsj.blogspot.com/2007/02/wcf-streaming-upload-files-over-http.html

                // Las configuraciones de "binding" del servidor no son visibles del lado del cliente a traves de WSDL,
                // segun http://social.msdn.microsoft.com/Forums/en-AU/wcf/thread/dcc46d86-87a5-4694-aa88-3568fddf159f.
                // Es necesario crear un nuevo binding del lado del cliente con las opciones de configuracion deseadas.

                Endpoint.Binding = new WSHttpBinding()
                {
                    Name = "bnd_GranTamano",
                    OpenTimeout = TimeSpan.Parse("00:00:30"),
                    CloseTimeout = TimeSpan.Parse("00:00:30"),
                    SendTimeout = TimeSpan.Parse("00:00:45"),
                    ReceiveTimeout = TimeSpan.Parse("00:00:45"),
                    MaxBufferPoolSize = 2147483647,
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas =
                    {
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 2147483647,
                        MaxDepth = 32,
                        MaxNameTableCharCount = 16384,
                        MaxStringContentLength = 2147483647
                    }
                };
            }
        }

        public void Conectar()
        {
            _IValeria = _Fabrica.CreateProxy("IValeria");
        }
        
        public DataSet InvocarRecibirTablas()
        {
            DataSet Resultado = null;
            Resultado = _IValeria.CallMethod("RecibirTablas", null) as DataSet;
            
            return Resultado;
        }
                
        public void InvocarEnviarTablas(string EsquemaXML, string XML)
        {
            _IValeria.CallMethod("EnviarTablas", EsquemaXML, XML);
        }

        public void Desconectar()
        {
            if (_IValeria != null)
            {
                _IValeria.Close();
                _IValeria = null;
            }
        }
        
        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

// http://www.google.co.ve/search?hl=es&client=firefox-a&hs=nJJ&rls=org.mozilla:es-ES:official&q=+site:social.msdn.microsoft.com+WCF+DynamicProxy+Library&sa=X&ei=txfjTp-0KrLE0AGKmoDNBQ&sqi=2&ved=0CDoQrQIwAg&biw=1366&bih=606#sclient=psy-ab&hl=es&client=firefox-a&hs=6JJ&rls=org.mozilla:es-ES%3Aofficial&source=hp&q=site:social.msdn.microsoft.com+WCF+DynamicProxy+Library+asynchronous&pbx=1&oq=site:social.msdn.microsoft.com+WCF+DynamicProxy+Library+asynchronous&aq=f&aqi=&aql=&gs_sm=e&gs_upl=29517l32601l0l32802l13l11l0l0l0l0l284l2401l0.3.8l11l0&bav=on.2,or.r_gc.r_pw.,cf.osb&fp=ecddcbf5a724ca0a&biw=1366&bih=606
// http://social.msdn.microsoft.com/Forums/en-US/wcf/thread/8cacd2e6-c817-4dd4-acc1-887c074ae3fe/
// http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/7697d769-782c-4f38-b930-8fbaa38833e1
