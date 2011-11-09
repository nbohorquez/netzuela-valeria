using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;    // ExploradorViewModel
using Zuliaworks.Netzuela.Valeria.Logica;                           // Nodo, Explorador ¡¡BORRAR!!

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Views
{
    /// <summary>
    /// Lógica de interacción para ExploradorView.xaml
    /// </summary>
    public partial class ExploradorView : UserControl
    {
        #region Constructores

        public ExploradorView()
        {
            InitializeComponent();
        }

        #endregion

        #region Funciones

        private void tvi_Item_Expandir(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = e.OriginalSource as TreeViewItem;
            if (Item == null)
                return;

            Item.IsExpanded = false;

            NodoViewModel ItemNodo = Item.DataContext as NodoViewModel;
            if (ItemNodo == null)
                return;

            TreeView TV = ArbolVisual.BusquedaHaciaArriba<TreeView>(Item as DependencyObject) as TreeView;
            ExploradorViewModel Arbol = TV.DataContext as ExploradorViewModel;

            Arbol.Expandir(ItemNodo);
        }

        private void tvi_Item_ClicBotonIzqRaton(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.IsExpanded = true;
        }

        private void tvi_Item_MouseEncima(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Italic;
        }

        private void tvi_Item_MouseAfuera(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Normal;
        }

        private void dgr_Tablas_ClicBotonIzqRaton(object sender, MouseButtonEventArgs e)
        {
            DataGridCell Celda = ArbolVisual.BusquedaHaciaArriba<DataGridCell>(e.OriginalSource as DependencyObject) as DataGridCell;

            if (Celda == null)
                return;

            DataGrid Grilla = ArbolVisual.BusquedaHaciaArriba<DataGrid>(Celda as DependencyObject) as DataGrid;

            // Hacemos que Arbol.NodoActual sea la columna seleccionada
            ExploradorViewModel Arbol = Grilla.DataContext as ExploradorViewModel;
            Arbol.NodoActual = NodoViewModelExtensiones.BuscarNodo(Celda.Column.Header as string, Arbol.NodoTablaActual.Hijos);
        }

        #endregion
    }
}
