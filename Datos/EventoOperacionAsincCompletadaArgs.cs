using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;    // AsyncCompletedEventArgs

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    public class EventoOperacionAsincCompletadaArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] Resultados;

        #endregion

        #region Constructores

        public EventoOperacionAsincCompletadaArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
            : base(Error, Cancelado, UsuarioID)
        {
            this.Resultados = Resultados;
        }

        #endregion

        #region Propiedades

        public object Resultado
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return Resultados[0];
            }
        }

        #endregion
    }
}
