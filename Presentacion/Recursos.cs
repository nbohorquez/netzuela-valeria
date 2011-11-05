using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // ConnectionState
using System.Globalization;                     // Culture
using System.Windows;                           // Visibility
using System.Windows.Controls;                  // DataGrid
using System.Windows.Input;                     // MouseButtonEventArgs
using System.Windows.Data;                      // IValueConverter, IMultiValueConverter
using System.Windows.Media;                     // VisualTreeHelper
using Zuliaworks.Netzuela.Valeria.Logica;                           // Nodo

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    public static class ArbolVisual
    {
        /*
         * Codigo tomado de:
         * http://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu
         */

        public static DependencyObject BusquedaHaciaArriba<T>(DependencyObject Origen)
        {
            while (Origen != null && Origen.GetType() != typeof(T))
                Origen = VisualTreeHelper.GetParent(Origen);

            return Origen;
        }
    }

    /*
     * Este convertidor lo uso para transformar ConnectionState a palabras
     */

    public class EstadoDeConexionBarraDeEstado : IValueConverter
    {
        public object Convert(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            ConnectionState Estado = (ConnectionState)Valor;
            object Resultado = null;

            switch (Estado)
            {
                case ConnectionState.Broken:
                    Resultado = "Conexión: Rota";
                    break;
                case ConnectionState.Closed:
                    Resultado = "Conexión: Cerrada";
                    break;
                case ConnectionState.Connecting:
                    Resultado = "Conexión: Conectando...";
                    break;
                case ConnectionState.Executing:
                    Resultado = "Conexión: Ejecutando";
                    break;
                case ConnectionState.Fetching:
                    Resultado = "Conexión: Recibiendo";
                    break;
                case ConnectionState.Open:
                    Resultado = "Conexión: Establecida";
                    break;
                default:
                    break;
            }
            return Resultado;
        }

        public object ConvertBack(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }

    /*
     * Este convertidor lo empleo para mostrar en pantalla qué columna destino esta asociado a cuál 
     * columna origen
     */

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

    /*
     * Este convertidor sirve para seleccionar toda la columna especificada por NodoActual
     */

    public class SeleccionarTodaLaColumna : IMultiValueConverter
    {
        public object Convert(object[] Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            Nodo NodoActual = Valor[0] as Nodo;
            DataGrid Grilla = Valor[1] as DataGrid;
            int Columna = -1;

            for (int i = 0; i < Grilla.Columns.Count; i++)
            {
                if (NodoActual.Nombre == Grilla.Columns[i].Header as string)
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
