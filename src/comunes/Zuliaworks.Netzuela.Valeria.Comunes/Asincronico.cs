namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;               // HybridDictionary
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.Linq;
    using System.Text;
    using System.Threading;                             // SendOrPostCallback

    public abstract class Asincronico : Desechable
    {
        #region Variables y Constantes

        private DelegadoComenzarOperacion carpintero;
        private Dictionary<string, Delegate> tareas;
        private Dictionary<string, Type> eventos;
        private Dictionary<string, SendOrPostCallback> retornos;
        private HybridDictionary hilos;
        private delegate void DelegadoComenzarOperacion(AsyncOperation asincronico, string operacion, object[] parametros);

        #endregion

        #region Constructores

        public Asincronico()
        {
            this.InicializarDelegados();
            this.tareas = new Dictionary<string, Delegate>();
            this.eventos = new Dictionary<string, Type>();
            this.retornos = new Dictionary<string, SendOrPostCallback>();
        }

        ~Asincronico()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        public void CrearTareaAsincronica<T>(object tareaId, Delegate tarea, SendOrPostCallback retorno, params object[] parametros) 
            where T : EventArgs
        {
            string nombre = tarea.Method.Name;
            
            try
            {
                AsyncOperation asincronico = this.RegistarTareaAsincronica(tareaId);
                this.tareas.Add(nombre, tarea);
                this.eventos.Add(nombre, typeof(T));
                this.retornos.Add(nombre, retorno);
                this.carpintero.BeginInvoke(asincronico, nombre, parametros, null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar la operacion asincrónica \"" + nombre + "\"", ex);
            }
        }

        public void CancelarTarea(object tareaId)
        {
            try
            {
                AsyncOperation asincronico = (AsyncOperation)this.hilos[tareaId];

                if (asincronico != null)
                {
                    lock (this.hilos.SyncRoot)
                    {
                        this.hilos.Remove(tareaId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelando la tarea " + tareaId.ToString(), ex);
            }
        }

        public bool TareaCancelada(object tareaId)
        {
            return this.hilos[tareaId] == null;
        }

        private AsyncOperation RegistarTareaAsincronica(object tareaId)
        {
            AsyncOperation asincronico = null;

            lock (this.hilos.SyncRoot)
            {
                if (this.hilos.Contains(tareaId))
                {
                    throw new ArgumentException("El argumento tareaId debe ser unico", "tareaId");
                }
                else
                {
                    asincronico = AsyncOperationManager.CreateOperation(tareaId);
                    this.hilos[tareaId] = asincronico;
                }
            }

            return asincronico;
        }

        private void ComenzarOperacion(AsyncOperation asincronico, string operacion, object[] parametros)
        {
            List<object> resultados = new List<object>();
            Exception e = null;

            if (!TareaCancelada(asincronico.UserSuppliedState))
            {
                try
                {
                    resultados.Add(this.tareas[operacion].DynamicInvoke(parametros));
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            this.FinalizarOperacion(operacion, resultados.ToArray(), e, TareaCancelada(asincronico.UserSuppliedState), asincronico);
        }

        private void FinalizarOperacion(string operacion, object[] resultados, Exception error, bool cancelado, AsyncOperation asincronico)
        {
            if (!cancelado)
            {
                lock (this.hilos.SyncRoot)
                {
                    this.hilos.Remove(asincronico.UserSuppliedState);
                }
            }

            var parametros = new object[4] { resultados, cancelado, error, asincronico.UserSuppliedState };
            var e = Activator.CreateInstance(this.eventos[operacion], parametros);
            asincronico.PostOperationCompleted(this.retornos[operacion], e);
        }

        protected virtual void InicializarDelegados()
        {
            this.carpintero = new DelegadoComenzarOperacion(ComenzarOperacion);
        }

        #endregion

        #region Implementación de interfaces

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            if (borrarCodigoAdministrado)
            {
                if (this.hilos != null)
                {
                    foreach (var entrada in this.hilos.Keys)
                    {
                        this.CancelarTarea(entrada);
                    }

                    this.hilos.Clear();
                    this.hilos = null;
                }

                if (this.retornos != null)
                {
                    this.retornos.Clear();
                }

                if (this.tareas != null)
                {
                    this.tareas.Clear();
                }

                if (this.eventos != null)
                {
                    this.eventos.Clear();
                }
            }
        }

        #endregion
    }
}
