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

using MS.Internal.WindowsBase;
using System.Data;                                                  // DataRowView
using System.Diagnostics;                                           // Debug
using System.Reflection;                                            // FieldInfo
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
            TreeViewItem Item = (TreeViewItem)ArbolVisual.BusquedaHaciaArriba<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (Item != null)
            {
                // El cambio de false a true de IsExpanded dispara el evento Expanded
                Item.IsExpanded = false;
                Item.IsExpanded = true;
            }
        }

        private void tvi_Item_MouseEncima(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = (TreeViewItem)ArbolVisual.BusquedaHaciaArriba<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Italic;
        }

        private void tvi_Item_MouseAfuera(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = (TreeViewItem)ArbolVisual.BusquedaHaciaArriba<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Normal;
        }

        private void dgr_Tabla_DestinoActualizado(object sender, DataTransferEventArgs e)
        {
            DataGrid Grilla = (DataGrid)ArbolVisual.BusquedaHaciaArriba<DataGrid>((DependencyObject)e.OriginalSource);

            // Codigo tomado del proyecto publicado en 
            // http://social.msdn.microsoft.com/Forums/en/wpf/thread/a5767cf4-8d26-4f72-b1b1-feca26bb6b2e
            
            FieldInfo selectionAnchorFieldInfo =
                typeof(DataGrid).GetField("_selectionAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(selectionAnchorFieldInfo != null, "El campo _selectionAnchor no existe en DataGrid");

            if (selectionAnchorFieldInfo != null)
            {
                selectionAnchorFieldInfo.SetValue(Grilla, null);
            }
        }
        
        #endregion
    }
}
