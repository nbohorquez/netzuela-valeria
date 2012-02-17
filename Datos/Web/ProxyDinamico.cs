using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WcfSamples.DynamicProxy;                          // DynamicProxyFactory
using System.Data;                                      // DataSet
using System.Security.Cryptography.X509Certificates;    // StoreLocation, StoreName, X509FindType
using System.ServiceModel;                              // WSHttpBinding
using System.ServiceModel.Description;                  // ServiceEndpoint
using System.ServiceModel.Dispatcher;                   // ClienRuntime
using System.ServiceModel.Security;                     // X509CertificateValidationMode
using Zuliaworks.Netzuela.Spuria.Api;                   // DataSetXml

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public class ProxyDinamico : IDisposable
    {
        /*
         * Con codigo de: http://blogs.msdn.com/b/vipulmodi/archive/2008/10/16/dynamic-proxy-and-memory-footprint.aspx
         */

        #region Variables

        private readonly WSHttpBinding _Vinculacion;
        private readonly WSHttpSecurity _Seguridad;
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

            _Seguridad = new WSHttpSecurity()
            {
                Mode = SecurityMode.Message,
                Message = { ClientCredentialType = MessageCredentialType.UserName }
            };

            _Vinculacion = new WSHttpBinding()
            {
                Name = "VinculacionConAutentificacion",
                Namespace = "http://netzuela.zuliaworks.com/spuria/api_publica",
                OpenTimeout = TimeSpan.Parse("00:00:30"),
                CloseTimeout = TimeSpan.Parse("00:00:30"),
                SendTimeout = TimeSpan.Parse("00:00:45"),
                ReceiveTimeout = TimeSpan.Parse("00:00:45"),
                MaxBufferPoolSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                Security = _Seguridad,
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

        private void Dispose(bool BorrarCodigoAdministrado)
        {
            this._Fabrica = null;
            this.Desconectar();

            if (BorrarCodigoAdministrado) 
            { 
            }
        }

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

                Endpoint.Binding = _Vinculacion;
                _ProxyDinamico = _Fabrica.CreateProxy(Contrato);

                var Credenciales = (ClientCredentials)_ProxyDinamico.GetProperty("ClientCredentials");
                Credenciales.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                
                // Esto es unicamente para las pruebas
                Credenciales.UserName.UserName = "molleja@abc.com";
                Credenciales.UserName.Password = "41ssdas#ASX";
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
                        DataTableXmlDinamico DataTableDinamico = new DataTableXmlDinamico(_Fabrica.ProxyAssembly);
                        DataTableDinamico.BaseDeDatos = ((DataTableXml)Argumentos[i]).BaseDeDatos;
                        DataTableDinamico.NombreTabla = ((DataTableXml)Argumentos[i]).NombreTabla;
                        DataTableDinamico.EsquemaXml = ((DataTableXml)Argumentos[i]).EsquemaXml;
                        DataTableDinamico.Xml = ((DataTableXml)Argumentos[i]).Xml;
                        DataTableDinamico.EstadoFilas = ((DataTableXml)Argumentos[i]).EstadoFilas;
                        DataTableDinamico.ClavePrimaria = ((DataTableXml)Argumentos[i]).ClavePrimaria;

                        Argumentos[i] = DataTableDinamico.ObjectInstance;
                    }
                }
            }

            try
            {
                Resultado = _ProxyDinamico.CallMethod(Metodo, Argumentos);
                
                // No puedo hacer "if(Resultado is DataSetXML)" por que ocurre este error:
                // http://stackoverflow.com/questions/2500280/invalidcastexception-for-two-objects-of-the-same-type
                
                if (Resultado.GetType().FullName == typeof(DataTableXml).FullName)
                {
                    DataTableXmlDinamico DataTableDinamico = new DataTableXmlDinamico(Resultado);
                    Resultado = new DataTableXml();

                    ((DataTableXml)Resultado).BaseDeDatos = DataTableDinamico.BaseDeDatos;
                    ((DataTableXml)Resultado).NombreTabla = DataTableDinamico.NombreTabla;
                    ((DataTableXml)Resultado).EsquemaXml = DataTableDinamico.EsquemaXml;
                    ((DataTableXml)Resultado).Xml = DataTableDinamico.Xml;
                    ((DataTableXml)Resultado).EstadoFilas = DataTableDinamico.EstadoFilas;
                    ((DataTableXml)Resultado).ClavePrimaria = DataTableDinamico.ClavePrimaria;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
