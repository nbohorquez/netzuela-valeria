using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;              // DynamicProxyFactory
using System.Data;                          // DataSet
using System.ServiceModel;                  //
using System.ServiceModel.Description;      // ServiceEndpoint

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    [Serializable]
    public class ProxyDinamico
    {
        // Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx

        #region Variable

        private string _UriWsdlServicio;
        private DynamicProxyFactory _Fabrica;

        #endregion

        #region Constructores
        
        public ProxyDinamico(string UriWsdlServicio)
        {
            _UriWsdlServicio = UriWsdlServicio;
            _Fabrica = new DynamicProxyFactory(_UriWsdlServicio);
        }

        #endregion

        #region Funciones

        public DataSet InvocarRecibirTablas()
        {
            DataSet Resultado;

            DynamicProxy RecibirTablas = _Fabrica.CreateProxy("IValeria");
            Resultado = RecibirTablas.CallMethod("RecibirTablas", null) as DataSet;
            RecibirTablas.Close();

            return Resultado;
        }

        public void InvocarEnviarTablas(string EsquemaXML, string XML)
        {
            DynamicProxy EnviarTablas = _Fabrica.CreateProxy("IValeria");
            EnviarTablas.CallMethod("EnviarTablas", EsquemaXML, XML);
            EnviarTablas.Close();
        }

        #endregion
    }
}
