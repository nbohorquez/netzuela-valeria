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

        private void tvi_Item_ClicBotonIzqRaton(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
            {
                // El cambio de false a true de IsExpanded dispara el evento Expanded
                Item.IsExpanded = false;
                Item.IsExpanded = true;
            }
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

        #endregion
    }
}
