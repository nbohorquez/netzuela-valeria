using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
using System.Threading;                             // Thread, SendOrPostCallback
using Zuliaworks.Netzuela.Paris.ContratoValeria;    // DataSetXML

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    public partial class ClienteValeria
    {
        #region Variables

        private SendOrPostCallback _DelegadoReportarEnvioDeTablasCompleado;
        private delegate void DelegadoComenzarEnviarTablas(DataSetXML Tablas, AsyncOperation Asincronico);

        #endregion

        #region Eventos

        public EventHandler<EventoEnviarTablasCompletadoArgs> EnviarTablasCompletado;

        #endregion

        #region Funciones

        #region Operaciones internas

        private void ComenzarEnviarTablas(DataSetXML Tablas, AsyncOperation Asincronico)
        {
            List<object> Resultados = new List<object>();
            Exception e = null;

            if (!TareaCancelada(Asincronico.UserSuppliedState))
            {
                try
                {
                    Resultados.Add(EnviarTablas(Tablas));
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            this.FinalizarEnviarTablas(Resultados.ToArray(), e, TareaCancelada(Asincronico.UserSuppliedState), Asincronico);
        }

        private void FinalizarEnviarTablas(object[] Resultados, Exception Error, bool Cancelado, AsyncOperation Asincronico)
        {
            if (!Cancelado)
            {
                lock (_Hilos.SyncRoot)
                {
                    _Hilos.Remove(Asincronico.UserSuppliedState);
                }
            }

            EventoEnviarTablasCompletadoArgs e =
                new EventoEnviarTablasCompletadoArgs(Resultados, Cancelado, Error, Asincronico.UserSuppliedState);

            Asincronico.PostOperationCompleted(_DelegadoReportarEnvioDeTablasCompleado, e);
        }
        
        private void AntesDeReportarEnvioDeTablasCompletado(object EstadoOperacion)
        {
            EventoEnviarTablasCompletadoArgs e = EstadoOperacion as EventoEnviarTablasCompletadoArgs;
            EnEnvioDeTablasCompletado(e);
        }

        protected void EnEnvioDeTablasCompletado(EventoEnviarTablasCompletadoArgs e)
        {
            if (EnviarTablasCompletado != null)
            {
                EnviarTablasCompletado(this, e);
            }
        }

        #endregion

        #region Métodos sincrónicos

        public bool EnviarTablas(DataSetXML Tablas)
        {
            //return Convert.ToBoolean(_Proxy.InvocarMetodo("EnviarTablas", Tablas.EsquemaXML, Tablas.XML));
            return Convert.ToBoolean(_Proxy.InvocarMetodo("EnviarTablas", Tablas));
        }

        #endregion

        #region Métodos asincrónicos

        public void EnviarTablasAsinc(DataSetXML Tablas)
        {
            EnviarTablasAsinc(Tablas, _Aleatorio.Next());
        }

        public void EnviarTablasAsinc(DataSetXML Tablas, object TareaID)
        {
            AsyncOperation Asincronico = AsyncOperationManager.CreateOperation(TareaID);

            lock (_Hilos.SyncRoot)
            {
                if (_Hilos.Contains(TareaID))
                    throw new ArgumentException("El argumento TareaID debe ser unico", "TareaID");

                _Hilos[TareaID] = Asincronico;
            }

            DelegadoComenzarEnviarTablas Carpintero = new DelegadoComenzarEnviarTablas(ComenzarEnviarTablas);
            Carpintero.BeginInvoke(Tablas, Asincronico, null, null);
        }
        
        #endregion

        #endregion
    }
}
