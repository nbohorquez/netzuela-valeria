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

using System.Drawing;           // Icon
using System.Windows.Forms;     // NotifyIcon

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /// <summary>
    /// Ventana de inicio del programa.
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: possemeeg
         * Titulo: Minimize to tray icon in WPF
         * Licencia: 
         * Fuente: http://possemeeg.wordpress.com/2007/09/06/minimize-to-tray-icon-in-wpf/
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

        private NotifyIcon _IconoDeBandeja;
        private WindowState _EstadoDeLaVentana;

        #endregion

        #region Constructores

        public VentanaPrincipal()
        {
            InitializeComponent();
            InicializarIconoDeBandeja();

            _EstadoDeLaVentana = WindowState.Normal;
        }

        #endregion

        #region Funciones
        
        private void InicializarIconoDeBandeja()
        {
            _IconoDeBandeja = new NotifyIcon();
            _IconoDeBandeja.BalloonTipText = "La aplicación ha sido minimizada. Haga clic en el ícono para restaurar la ventana";
            _IconoDeBandeja.BalloonTipTitle = "Netzuela";
            _IconoDeBandeja.Text = "Netzuela";
            _IconoDeBandeja.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Zuliaworks.Netzuela.Valeria.Presentacion.sharethis.ico"));

            _IconoDeBandeja.Click += new EventHandler(ClicSobreIcono);
        }

        private void ManejarVentanaCerrandose(object Remitente, System.ComponentModel.CancelEventArgs Argumentos)
        {
            _IconoDeBandeja.Dispose();
            _IconoDeBandeja = null;
        }

        private void ManejarCambioDeEstado(object Remitente, EventArgs Argumentos)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (_IconoDeBandeja != null)
                    _IconoDeBandeja.ShowBalloonTip(2000);
            }
            else
            {
                _EstadoDeLaVentana = WindowState;
            }
        }

        private void ManejarCambioDeVisibilidad(object Remitente, DependencyPropertyChangedEventArgs Argumentos)
        {
            if (_IconoDeBandeja != null)
                _IconoDeBandeja.Visible = !IsVisible;
        }

        private void ClicSobreIcono(object Remitente, EventArgs Argumentos)
        {
            Show();
            WindowState = _EstadoDeLaVentana;
        }

        #endregion
    }
}
