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
        private SendOrPostCallback _DelegadoReportarProgreso;

        #endregion

        #region Constructores

        public ClienteValeria(string UriWsdlServicio)
        {
            InicializarDelegados();

            _Aleatorio = new Random();
            _Hilos = new HybridDictionary();
            _Proxy = new ProxyDinamico(UriWsdlServicio);
        }

        #endregion

        #region Eventos

        public EventHandler<ProgressChangedEventArgs> CambioEnProgresoDeOperacion;

        #endregion

        #region Funciones
        
        private bool TareaCancelada(object TareaID)
        {
            return (_Hilos[TareaID] == null);
        }

        private void AntesDeReportarProgreso(object Estado)
        {
            ProgressChangedEventArgs e = Estado as ProgressChangedEventArgs;
            EnReportarProgreso(e);
        }

        protected void EnReportarProgreso(ProgressChangedEventArgs e)
        {
            if (CambioEnProgresoDeOperacion != null)
            {
                CambioEnProgresoDeOperacion(this, e);
            }
        }

        protected virtual void InicializarDelegados()
        {
            _DelegadoReportarProgreso = new SendOrPostCallback(AntesDeReportarProgreso);
            _DelegadoReportarEnvioDeTablasCompleado = new SendOrPostCallback(AntesDeReportarEnvioDeTablasCompletado);
        }

        public void Conectar()
        {
            if (_Proxy != null)
            {
                _Proxy.Conectar("IValeria");
            }
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
