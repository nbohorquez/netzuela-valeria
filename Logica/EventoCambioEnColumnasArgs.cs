namespace Zuliaworks.Netzuela.Valeria.Logica
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class EventoCambioEnColumnasArgs : EventArgs
    {
        #region Constructores

        public EventoCambioEnColumnasArgs(string columna, Nodo valorAnterior, Nodo valorActual)
        {
            this.Columna = columna;
            this.ValorAnterior = valorAnterior;
            this.ValorActual = valorActual;
        }

        #endregion

        #region Propiedades

        public string Columna { get; private set; }

        public Nodo ValorAnterior { get; private set; }

        public Nodo ValorActual { get; private set; }

        #endregion
    }
}
