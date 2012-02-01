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

using Zuliaworks.Netzuela.Valeria.Comunes;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Views
{
    /// <summary>
    /// Lógica de interacción para ConexionLocalView.xaml
    /// </summary>
    public partial class ConexionLocalView : UserControl
    {
        #region Constructores

        public ConexionLocalView()
        {
            InitializeComponent();
        }

        #endregion

        #region Funciones

        private void AutentificacionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            view_Autentificacion.txt_Usuario.Text = string.Empty;
            ((PasswordBox)view_Autentificacion.pwd_Contrasena.Child).Password = string.Empty;
        }

        #endregion
    }
}
