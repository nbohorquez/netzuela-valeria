using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;              // DynamicProxyFactory
using System.Data;                          // DataSet
using System.ServiceModel;                  //
using System.ServiceModel.Description;      // ServiceEndpoint
//using System.Xml;                           // 

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    [Serializable]
    public class ProxyDinamico
    {
        // Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx

        #region Variable

        private string _UriWsdlServicio;

        #endregion

        #region Constructores
        
        public ProxyDinamico(string UriWsdlServicio)
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");
                
            _UriWsdlServicio = UriWsdlServicio;
        }

        #endregion

        #region Propiedades

        public string EsquemaXML { get; set; }
        public string XML { get; set; }

        #endregion

        #region Funciones

        private DynamicProxy CrearProxy()
        {
            DynamicProxy Resultado = null;
            DynamicProxyFactory Fabrica = new DynamicProxyFactory(_UriWsdlServicio);
            ServiceEndpoint Endpoint = null;

            foreach (ServiceEndpoint SE in Fabrica.Endpoints)
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

                Resultado = Fabrica.CreateProxy("IValeria");
            }

            return Resultado;
        }

        public DataSet InvocarRecibirTablas()
        {
            DataSet Resultado = null;

            DynamicProxy RecibirTablas = CrearProxy();
            Resultado = RecibirTablas.CallMethod("RecibirTablas", null) as DataSet;
            RecibirTablas.Close();
            
            /*
            _IValeria = _Fabrica.CreateProxy("IValeria");
            Resultado = _IValeria.CallMethod("RecibirTablas", null) as DataSet;
            _IValeria.Close();
             * */
            return Resultado;
        }

        public void InvocarEnviarTablas()
        {
            DynamicProxy EnviarTablas = CrearProxy();            
            EnviarTablas.CallMethod("EnviarTablas", EsquemaXML, XML);
            EnviarTablas.Close();
        }
    
        /*
        public void InvocarEnviarTablas(string EsquemaXML, string XML)
        {
            _IValeria = _Fabrica.CreateProxy("IValeria");
            _IValeria.CallMethod("EnviarTablas", EsquemaXML, XML);
            _IValeria.Close();
        }
        */
        #endregion
    }
}
