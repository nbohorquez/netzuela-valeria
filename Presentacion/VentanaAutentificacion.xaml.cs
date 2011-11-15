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

using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;

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

            // Establezco el DataContext aqui porque necesito acceder a "CerrarView" desde la ventana.
            var ViewModel = new AutentificacionViewModel();
            this.DataContext = ViewModel;
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        // ...

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}