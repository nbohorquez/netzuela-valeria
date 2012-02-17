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

using System.Globalization;             // Culture

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Views
{
    /// <summary>
    /// Lógica de interacción para DetectarServidoresLocalesView.xaml
    /// </summary>
    public partial class DetectarServidoresLocalesView : UserControl
    {
        public DetectarServidoresLocalesView()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// La siguiente clase se utiliza para mostrar contenido por defecto cuando no hay nada 
    /// seleccionado en un ComboBox. El valor devuelto controla la propiedad "Visibility" de 
    /// un TextBox.
    /// </summary>
    class TextoPorDefectoEnComboBox1 : IValueConverter
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: Tri Q
         * Titulo: How to display default text “--Select Team --” in combo box on pageload in WPF? 
         *      (pregunta en el foro "stackoverflow)
         * Licencia: Creative Commons Attribution-ShareAlike 3.0 Unported
         * Fuente: http://stackoverflow.com/questions/1426050/how-to-display-default-text-select-team-in-combo-box-on-pageload-in-wpf
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              [X]
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       []
         * 
         */

        /*
         * Estas clases no estan anidadas a DetectarServidoresLocalesView (que seria lo ideal)
         * por la siguiente razon:
         * http://stackoverflow.com/questions/4269896/creating-an-instance-of-a-nested-class-in-xaml
         */

        public object Convert(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            return Valor == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// La siguiente clase se utiliza para mostrar contenido por defecto cuando no hay nada 
    /// seleccionado en un ComboBox. El valor devuelto controla la propiedad "Visibility" de 
    /// un TextBox.
    /// </summary>
    class TextoPorDefectoEnComboBox2 : IMultiValueConverter
    {
        public object Convert(object[] Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            if (Valor[0] != null)
            {
                return Valor[1] == null ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object Valor, Type[] TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }
}
