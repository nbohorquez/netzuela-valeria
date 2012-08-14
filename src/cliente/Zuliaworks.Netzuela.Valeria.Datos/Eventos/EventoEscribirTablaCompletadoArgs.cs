namespace Zuliaworks.Netzuela.Valeria.Datos.Eventos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;            // AsyncCompletedEventArgs
    using System.Linq;
    using System.Text;

    public class EventoEscribirTablaCompletadoArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] resultados;

        #endregion

        #region Constructores

        public EventoEscribirTablaCompletadoArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
            : base(Error, Cancelado, UsuarioID)
        {
            this.resultados = Resultados;
        }

        #endregion

        #region Propiedades

        public bool Resultado
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return Convert.ToBoolean(this.resultados[0]);
            }
        }
            
        #endregion
    }
}
