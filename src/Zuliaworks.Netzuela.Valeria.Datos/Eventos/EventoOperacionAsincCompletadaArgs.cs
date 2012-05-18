namespace Zuliaworks.Netzuela.Valeria.Datos.Eventos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;            // AsyncCompletedEventArgs
    using System.Linq;
    using System.Text;

    public class EventoOperacionAsincCompletadaArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] resultados;

        #endregion

        #region Constructores

        public EventoOperacionAsincCompletadaArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
            : base(Error, Cancelado, UsuarioID)
        {
            this.resultados = Resultados;
        }

        #endregion
        /*
        #region Propiedades

        public object Resultado
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return resultados[0];
            }
        }

        #endregion
         */
    }
}
