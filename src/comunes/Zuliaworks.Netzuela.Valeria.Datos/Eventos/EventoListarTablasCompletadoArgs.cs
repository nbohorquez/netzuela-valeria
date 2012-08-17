namespace Zuliaworks.Netzuela.Valeria.Datos.Eventos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;            // AsyncCompletedEventArgs
    using System.Linq;
    using System.Text;

    public class EventoListarTablasCompletadoArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] resultados;

        #endregion

        #region Constructores

        public EventoListarTablasCompletadoArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
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
                this.RaiseExceptionIfNecessary();
                return (string[])this.resultados[0];
            }
        }

        #endregion
    }
}