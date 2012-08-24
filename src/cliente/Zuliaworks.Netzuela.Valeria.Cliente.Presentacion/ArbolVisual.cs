namespace Zuliaworks.Netzuela.Valeria.Cliente.Presentacion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;                           // DependencyObject
    using System.Windows.Media;                     // VisualTreeHelper

    /// <summary>
    /// Contiene funciones para los objetos de un arbol visual
    /// </summary>
    public static class ArbolVisual
    {
        #region Funciones

        /* 
         * Codigo importado
         * ================
         * 
         * Autor: alex2k8
         * Titulo: Select TreeView Node on right click before displaying ContextMenu (Pregunta en el foro "stackoverflow") 
         * Licencia: Creative Commons Attribution-ShareAlike 3.0 Unported
         * Fuente: http://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              []
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       [X]
         * 
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