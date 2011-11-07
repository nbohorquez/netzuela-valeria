using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;                     // Culture
using System.Windows.Data;                      // IValueConverter, IMultiValueConverter
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /*
     * Este convertidor lo uso para definir la ruta del nodo actual seleccionado en txt_ElementoActual
     */

    public class DeclaracionRutaDeNodo : IValueConverter
    {
        public object Convert(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            Nodo Nodo = (Nodo)Valor;
            return Nodo.RutaCompleta();
        }

        public object ConvertBack(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }
}
