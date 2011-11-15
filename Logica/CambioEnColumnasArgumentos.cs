using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    public class CambioEnColumnasArgumentos
    {
        #region Constructores

        public CambioEnColumnasArgumentos(string Columna, Nodo ValorAnterior, Nodo ValorActual)
        {
            this.Columna = Columna;
            this.ValorAnterior = ValorAnterior;
            this.ValorActual = ValorActual;
        }

        #endregion

        #region Propiedades

        public string Columna { get; private set; }
        public Nodo ValorAnterior { get; private set; }
        public Nodo ValorActual { get; private set; }

        #endregion
    }
}
