using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;                      // DynamicProxyFactory
using System.Data;                                  // DataSet
using System.ServiceModel;                          // WSHttpBinding
using System.ServiceModel.Description;              // ServiceEndpoint
using Zuliaworks.Netzuela.Paris.ContratoValeria;    // DataSetXML

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    //[Serializable]
    public class ProxyDinamico : IDisposable
    {
        // Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx

        #region Variable

        private string _UriWsdlServicio;
        private DynamicProxyFactory _Fabrica;
        private DynamicProxy _ProxyDinamico;

        #endregion

        #region Constructores
        
        public ProxyDinamico(string UriWsdlServicio)
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");
                
            _UriWsdlServicio = UriWsdlServicio;

            // En este instruccion es donde se consume la mayor cantidad de tiempo de ejecucion
            _Fabrica = new DynamicProxyFactory(_UriWsdlServicio);
        }

        ~ProxyDinamico()
        {
            Dispose(false);
        }

        #endregion
        
        #region Funciones

        private void Dispose(bool BorrarCodigoAdministrado)
        {
            Desconectar();

            if (BorrarCodigoAdministrado) { }
        }
        
        public void Conectar(string Contrato)
        {
            ServiceEndpoint Endpoint = null;

            foreach (ServiceEndpoint SE in _Fabrica.Endpoints)
            {
                if (SE.Contract.Name.Contains(Contrato))
                {
                    Endpoint = SE;
                }
            }

            if (Endpoint != null)
            {
                // Al usar WSHttpBinding se pierde la capacidad de enviar los datos en un flujo sin fin 
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

            _ProxyDinamico = _Fabrica.CreateProxy(Contrato);
        }
        /*
        public DataSet InvocarRecibirTablas()
        {
            DataSet Resultado = null;
            Resultado = _Proxy.CallMethod("RecibirTablas", null) as DataSet;
            
            return Resultado;
        }
               
        public void InvocarEnviarTablas(string EsquemaXML, string XML)
        {
            _Proxy.CallMethod("EnviarTablas", EsquemaXML, XML);
        }
        */
        //public object InvocarMetodo(string Metodo, params object[] Argumentos)
        public object InvocarMetodo(string Metodo, DataSetXML Argumentos)
        {
            //DataSetXML XML = Argumentos[0] as DataSetXML;
            //return _ProxyDinamico.CallMethod(Metodo, Argumentos);
            return _ProxyDinamico.CallMethod(Metodo, Argumentos);
        }

        public void Desconectar()
        {
            if (_ProxyDinamico != null)
            {
                _ProxyDinamico.Close();
                _ProxyDinamico = null;
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
