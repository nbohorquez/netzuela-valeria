using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security;                      // SecureString
using System.Windows;                       // DependencyProperty y otros demonios...
using System.Windows.Controls;              // SetValue, GetValue ...

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Esta clase se emplea para crear la "dependency property" SecurePassword en PasswordBox 
    /// y asi poder vincularse (to bind) a ella desde el ViewModel.
    /// </summary>
    public class PasswordBoxVinculable : Decorator
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: Nir
         * Titulo: PasswordBox with MVVM (Pregunta en el foro "stackoverflow") 
         * Licencia: Creative Commons Attribution-ShareAlike 3.0 Unported
         * Fuente: http://stackoverflow.com/questions/1097235/passwordbox-with-mvvm
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              []
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       [X]
         * 
         */

        #region Variables

        public static readonly DependencyProperty SecurePasswordPropiedad =
            DependencyProperty.Register("SecurePassword", typeof(SecureString), typeof(PasswordBoxVinculable));

        #endregion

        #region Constructores

        public PasswordBoxVinculable()
        {
            Child = new PasswordBox();
            ((PasswordBox)Child).PasswordChanged += ManejarPasswordChanged;
        }

        #endregion

        #region Propiedades

        public SecureString SecurePassword
        {
            get { return (SecureString)GetValue(SecurePasswordPropiedad); }
            set { SetValue(SecurePasswordPropiedad, value); }
        }

        #endregion

        #region Funciones

        void ManejarPasswordChanged(object sender, RoutedEventArgs e)
        {
            SecurePassword = ((PasswordBox)Child).SecurePassword;
        }

        #endregion
    }
}
