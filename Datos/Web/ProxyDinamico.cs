using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;                      // DynamicProxyFactory
using System.Data;                                  // DataSet
using System.ServiceModel;                          // WSHttpBinding
using System.ServiceModel.Description;              // ServiceEndpoint
using Zuliaworks.Netzuela.Spuria.Contrato;          // DataSetXML

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public class ProxyDinamico : IDisposable
    {
        // Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx

        #region Variables

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
            CrearFabrica();
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
                        throw new FieldAccessException("No se puede modificar UriWsdlServicio mientras la conexión este establecida");
                }
            }
        }

        #endregion

        #region Funciones

        private void Dispose(bool BorrarCodigoAdministrado)
        {
            Desconectar();

            if (BorrarCodigoAdministrado) { }
        }

        private void CrearFabrica()
        {
            // En este instruccion es donde se consume la mayor cantidad de tiempo de ejecucion
            _Fabrica = new DynamicProxyFactory(UriWsdlServicio);
        }

        public void Conectar(string Contrato)
        {
            Desconectar();

            if (Contrato == null)
                throw new ArgumentNullException("Contrato");

            try
            {
                if (_UriWsdlServicioModificado)
                {
                    _UriWsdlServicioModificado = false;

                    if (UriWsdlServicio == null)
                        throw new ArgumentNullException("UriWsdlServicio");

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
                    if (Argumentos[i] is DataSetXML)
                    {
                        DataSetXMLDinamico DataSetDinamico = new DataSetXMLDinamico(_Fabrica.ProxyAssembly);
                        DataSetDinamico.BaseDeDatos = ((DataSetXML)Argumentos[i]).BaseDeDatos;
                        DataSetDinamico.NombreTabla = ((DataSetXML)Argumentos[i]).NombreTabla;
                        DataSetDinamico.EsquemaXML = ((DataSetXML)Argumentos[i]).EsquemaXML;
                        DataSetDinamico.XML = ((DataSetXML)Argumentos[i]).XML;

                        Argumentos[i] = DataSetDinamico.ObjectInstance;
                    }
                }
            }

            try
            {
                Resultado = _ProxyDinamico.CallMethod(Metodo, Argumentos);
                
                // No puedo hacer "if(Resultado is DataSetXML)" por que ocurre este error:
                // http://stackoverflow.com/questions/2500280/invalidcastexception-for-two-objects-of-the-same-type
                if (Resultado.GetType().FullName == typeof(DataSetXML).FullName)
                {
                    DataSetXMLDinamico SetXMLDinamico = new DataSetXMLDinamico(Resultado);
                    Resultado = new DataSetXML();

                    ((DataSetXML)Resultado).BaseDeDatos = SetXMLDinamico.BaseDeDatos;
                    ((DataSetXML)Resultado).NombreTabla = SetXMLDinamico.NombreTabla;
                    ((DataSetXML)Resultado).EsquemaXML = SetXMLDinamico.EsquemaXML;
                    ((DataSetXML)Resultado).XML = SetXMLDinamico.XML;
                }
            }
            catch (Exception ex)
            {
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
