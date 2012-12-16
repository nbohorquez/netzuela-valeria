namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;               // HybridDictionary
    using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
    using System.Linq;
    using System.Text;

    public abstract class Asincrono : Desechable
    {
        #region Variables y Constantes

        private DelegadoComenzarOperacion carpintero;
        private delegate void DelegadoComenzarOperacion(AsyncOperation asincronico, string operacion, object[] parametros);
        private Dictionary<string, Delegate> tareas;
        private HybridDictionary hilos;

        #endregion

        #region Constructores

        public Asincrono()
        {
            this.InicializarDelegados();
        }

        #endregion

        #region Funciones

        public void TareaAsincronica(object tareaId, Delegate tarea, params object[] parametros)
        {
            string nombre = tarea.Method.Name;
            
            try
            {
                AsyncOperation asincronico = this.CrearOperacionAsincronica(tareaId);
                this.tareas.Add(nombre, tarea);
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

        private AsyncOperation CrearOperacionAsincronica(object tareaId)
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

            switch (operacion)
            {
                case "ListarTiendas":
                    {
                        EventoListarTiendasCompletadoArgs e =
                            new EventoListarTiendasCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarTiendasCompletado, e);
                        break;
                    }

                case "ListarBasesDeDatos":
                    {
                        EventoListarBDsCompletadoArgs e =
                            new EventoListarBDsCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarBDsCompletado, e);
                        break;
                    }

                case "ListarTablas":
                    {
                        EventoListarTablasCompletadoArgs e =
                            new EventoListarTablasCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararListarTablasCompletado, e);
                        break;
                    }

                case "LeerTabla":
                    {
                        EventoLeerTablaCompletadoArgs e =
                            new EventoLeerTablaCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararLeerTablaCompletado, e);
                        break;
                    }

                case "EscribirTabla":
                    {
                        EventoEscribirTablaCompletadoArgs e =
                            new EventoEscribirTablaCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararEscribirTablaCompletado, e);
                        break;
                    }

                case "CrearUsuario":
                    {
                        EventoCrearUsuarioCompletadoArgs e =
                            new EventoCrearUsuarioCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararCrearUsuarioCompletado, e);
                        break;
                    }

                case "Consultar":
                    {
                        EventoConsultarCompletadoArgs e =
                            new EventoConsultarCompletadoArgs(resultados, cancelado, error, asincronico.UserSuppliedState);
                        asincronico.PostOperationCompleted(this.delegadoDispararConsultarCompletado, e);
                        break;
                    }

                default:
                    break;
            }
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
            }
        }

        #endregion
    }
}
