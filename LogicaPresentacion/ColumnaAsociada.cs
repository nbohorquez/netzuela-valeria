using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;                     // Culture
using System.Windows.Data;                      // IMultiValueConverter
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Este convertidor lo empleo para mostrar en pantalla qué columna destino esta asociado a cuál 
    /// columna origen
    /// </summary>
    public class ColumnaAsociada : IMultiValueConverter
    {
        public object Convert(object[] Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            Nodo Nodo = Valor[0] as Nodo;
            Nodo ColOrg = Valor[1] as Nodo;
            Nodo ColDst = Valor[2] as Nodo;

            string Resultado = "";

            if (Nodo == ColDst)
            {
                Resultado += ColOrg == null ? "" : "<-" + ColOrg.Nombre;
            }
            else if (Nodo == ColOrg)
            {
                Resultado += ColDst == null ? "" : "->" + ColDst.Nombre;
            }

            return Resultado;
        }

        public object[] ConvertBack(object Valor, Type[] TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }
}
