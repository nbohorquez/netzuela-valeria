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
using System.Windows.Shapes;

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /// <summary>
    /// Pide al usuario el nombre de la cuenta y la contraseña asociada para poder conectarse 
    /// al SGBDR seleccionado.
    /// </summary>
    public partial class VentanaAutentificacion : Window
    {
        #region Variables
        
        // ...

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public VentanaAutentificacion()
        {
            InitializeComponent();
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void btn_Acceder_Clic(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tcl_Enter_Presionado(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.Close();
            }
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
