using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;                           // DependencyObject
using System.Windows.Media;                     // VisualTreeHelper

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Contiene funciones para los objetos de un arbol visual
    /// </summary>
    public static class ArbolVisual
    {
        #region Funciones

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

        #endregion
    }
}
