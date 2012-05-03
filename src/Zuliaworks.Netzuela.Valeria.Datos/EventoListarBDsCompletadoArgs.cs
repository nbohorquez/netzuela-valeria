namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;            // AsyncCompletedEventArgs
    using System.Linq;
    using System.Text;

    public class EventoListarBDsCompletadoArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] resultados;

        #endregion

        #region Constructores

        public EventoListarBDsCompletadoArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
            : base(Error, Cancelado, UsuarioID)
        {
            this.resultados = Resultados;
        }

        #endregion

        #region Propiedades

        public string[] Resultado
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return (string[])resultados[0];
            }
        }

        #endregion
    }
}
