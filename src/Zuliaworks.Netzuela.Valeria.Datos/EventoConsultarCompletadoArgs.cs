﻿namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;            // AsyncCompletedEventArgs
    using System.Data;
    using System.Linq;
    using System.Text;

    public class EventoConsultarCompletadoArgs : AsyncCompletedEventArgs
    {
        #region Variables

        private object[] resultados;

        #endregion

        #region Constructores

        public EventoConsultarCompletadoArgs(object[] Resultados, bool Cancelado, Exception Error, object UsuarioID)
            : base(Error, Cancelado, UsuarioID)
        {
            this.resultados = Resultados;
        }

        #endregion

        #region Propiedades

        public DataTable Resultado
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return (DataTable)this.resultados[0];
            }
        }
            
        #endregion
    }
}
