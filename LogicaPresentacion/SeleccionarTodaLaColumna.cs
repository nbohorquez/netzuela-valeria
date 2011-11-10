using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;                                         // Culture
using System.Windows.Controls;                                      // DataGrid
using System.Windows.Data;                                          // IMultiValueConverter
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;    // NodoViewModel

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Este convertidor sirve para seleccionar toda la columna especificada por NodoActual
    /// </summary>
    public class SeleccionarTodaLaColumna : IMultiValueConverter
    {
        public object Convert(object[] Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            string NombreNodoActual = Valor[0] as string;
            DataGrid Grilla = Valor[1] as DataGrid;
            int Columna = -1;

            for (int i = 0; i < Grilla.Columns.Count; i++)
            {
                if (NombreNodoActual == Grilla.Columns[i].Header as string)
                {
                    Columna = i;
                    break;
                }
            }

            // Seleccionamos todas las celdas de esa columna
            if (Columna != -1)
            {
                Grilla.SelectedCells.Clear();
                for (int i = 0; i < Grilla.Items.Count; i++)
                {
                    DataGridCellInfo iCelda = new DataGridCellInfo(Grilla.Items[i], Grilla.Columns[Columna]);
                    Grilla.SelectedCells.Add(iCelda);
                }
            }

            return Grilla.CurrentCell;
        }

        public object[] ConvertBack(object Valor, Type[] TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }
}
