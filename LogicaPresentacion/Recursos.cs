using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Security;
using System.Windows.Controls;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    /// <summary>
    /// Este clase se emplea para cerrar los dialogos desde los ViewModel.
    /// </summary>
    public static class DialogCloser
    {
        // Este codigo fue tomado de http://blog.excastle.com/2010/07/25/mvvm-and-dialogresult-with-no-code-behind/
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool?),
            typeof(DialogCloser),
            new PropertyMetadata(DialogResultChanged)
        );

        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
            {
                window.DialogResult = e.NewValue as bool?;
            }
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }

    public class BindablePasswordBox : Decorator
    {
        // Este codigo fue tomado de http://stackoverflow.com/questions/1097235/passwordbox-with-mvvm
        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.Register("SecurePassword", typeof(SecureString), typeof(BindablePasswordBox));

        public SecureString SecurePassword
        {
            get { return (SecureString)GetValue(SecurePasswordProperty); }
            set { SetValue(SecurePasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            Child = new PasswordBox();
            ((PasswordBox)Child).PasswordChanged += BindablePasswordBox_PasswordChanged;
        }

        void BindablePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SecurePassword = ((PasswordBox)Child).SecurePassword;
        }
    }
}
