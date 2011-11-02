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

    /*
     * Las siguiente clases se utilizan para mostrar contenido por defecto cuando no hay nada 
     * seleccionado en un ComboBox. El valor devuelto controla la propiedad "Visibility" de un 
     * TextBox. Ver el codigo local VentanaDetectarServidoresLocales.xaml y la explicacion de 
     * la pagina:
     * 
     * http://stackoverflow.com/questions/1426050/how-to-display-default-text-select-team-in-combo-box-on-pageload-in-wpf
     * 
     * Estas clases no estan anidadas a DetectarServidoresLocalesView (que seria lo ideal) por 
     * la siguiente razon:
     * 
     * http://stackoverflow.com/questions/4269896/creating-an-instance-of-a-nested-class-in-xaml
     */

    class TextoPorDefectoEnComboBox1 : IValueConverter
    {
        public object Convert(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            return Valor == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object Valor, Type TipoObjetivo, object Parametro, CultureInfo Cultura)
        {
            throw new NotImplementedException();
        }
    }

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
