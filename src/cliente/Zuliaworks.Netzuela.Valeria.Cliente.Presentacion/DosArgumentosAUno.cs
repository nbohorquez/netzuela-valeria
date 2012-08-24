namespace Zuliaworks.Netzuela.Valeria.Cliente.Presentacion
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;                     // Culture
    using System.Linq;
    using System.Text;
    using System.Windows.Data;                      // IMultiValueConverter

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
