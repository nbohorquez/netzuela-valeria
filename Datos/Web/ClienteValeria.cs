using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;               // HybridDictionary
using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
using System.Threading;                             // Thread, SendOrPostCallback
using Zuliaworks.Netzuela.Paris.ContratoValeria;    // DataSetXML

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ClienteValeria : Component
    {
        // Ver primero: http://msdn.microsoft.com/en-us/library/bz33kx67.aspx
        // Ver tambien: http://msdn.microsoft.com/en-us/library/wewwczdw.aspx
        
        #region Variables

        private Random _Aleatorio;
        private ProxyDinamico _Proxy;
        private HybridDictionary _Hilos;

        #endregion

        #region Constructores

        public ClienteValeria()
        {
            InicializarDelegados();

            _Aleatorio = new Random();
            _Hilos = new HybridDictionary();
        }

        public ClienteValeria(string UriWsdlServicio)
            : this()
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");

            this.UriWsdlServicio = UriWsdlServicio;
            CrearProxy();
        }

        #endregion

        #region Propiedades

        public string UriWsdlServicio { get; set; }

        #endregion

        #region Funciones

        private void CrearProxy()
        {
            try
            {
                _Proxy = new ProxyDinamico(UriWsdlServicio);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando ProxyDinamico con argumento: \"" + UriWsdlServicio + "\"", ex);
            }
        }

        private bool TareaCancelada(object TareaID)
        {
            return (_Hilos[TareaID] == null);
        }

        protected virtual void InicializarDelegados()
        {
            _DelegadoDispararCambioEnProgresoDeOperacion = new SendOrPostCallback(AntesDeDispararCambioEnProgresoDeOperacion);
            _DelegadoDispararListarBDsCompletado = new SendOrPostCallback(AntesDeDispararListarBDsCompletado);
            _DelegadoDispararListarTablasCompletado = new SendOrPostCallback(AntesDeDispararListarTablasCompletado);
            _DelegadoDispararLeerTablaCompletado = new SendOrPostCallback(AntesDeDispararLeerTablaCompletado);
            _DelegadoDispararEscribirTablaCompletado = new SendOrPostCallback(AntesDeDispararEscribirTablaCompletado);
            _DelegadoDispararCrearUsuarioCompletado = new SendOrPostCallback(AntesDeDispararCrearUsuarioCompletado);
            _Carpintero = new DelegadoComenzarOperacion(ComenzarOperacion);
        }

        public void Conectar()
        {
            if (_Proxy == null)
            {
                if(UriWsdlServicio == null)
                    throw new ArgumentNullException("UriWsdlServicio");

                CrearProxy();
            }

            _Proxy.Conectar("IValeria");            
        }

        public void Desconectar()
        {
            if (_Proxy != null)
            {
                _Proxy.Desconectar();
            }
        }

        public void CancelarTarea(object TareaID)
        {
            AsyncOperation Asincronico = _Hilos[TareaID] as AsyncOperation;

            if (Asincronico != null)
            {
                lock (_Hilos.SyncRoot)
                {
                    _Hilos.Remove(TareaID);
                }
            }
        }

        #endregion
    }
}
