using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;           // DependencyProperty y otros demonios...

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
}
