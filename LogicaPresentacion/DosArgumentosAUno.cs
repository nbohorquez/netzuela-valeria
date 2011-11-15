using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;                     // Culture
using System.Windows.Data;                      // IMultiValueConverter

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    public class DosArgumentosAUno : IMultiValueConverter
    {
        #region Funciones

        public object Convert(object[] Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            return Valor.Clone();
        }

        public object[] ConvertBack(object Valor, Type[] TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

