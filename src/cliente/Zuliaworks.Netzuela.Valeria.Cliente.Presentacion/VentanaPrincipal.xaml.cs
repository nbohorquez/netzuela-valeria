namespace Zuliaworks.Netzuela.Valeria.Cliente.Presentacion
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;                   // Icon
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Forms;             // NotifyIcon
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

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

        private NotifyIcon iconoDeBandeja;
        private WindowState estadoDeLaVentana;

        #endregion

        #region Constructores

        public VentanaPrincipal()
        {
            this.InitializeComponent();
            this.InicializarIconoDeBandeja();

            this.estadoDeLaVentana = WindowState.Normal;
        }

        #endregion

        #region Funciones
        
        private void InicializarIconoDeBandeja()
        {
            this.iconoDeBandeja = new NotifyIcon();
            this.iconoDeBandeja.BalloonTipText = "La aplicación ha sido minimizada. Haga clic en el ícono para restaurar la ventana";
            this.iconoDeBandeja.BalloonTipTitle = "Netzuela";
            this.iconoDeBandeja.Text = "Netzuela";
            // Para hacer esto teneis que acordarte que Logo.png debe ser un "recurso incrustado"
            this.iconoDeBandeja.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Zuliaworks.Netzuela.Valeria.Cliente.Presentacion.Logo.ico"));

            this.iconoDeBandeja.Click += new EventHandler(this.ClicSobreIcono);
        }

        private void ManejarVentanaCerrandose(object remitente, System.ComponentModel.CancelEventArgs argumentos)
        {
            this.iconoDeBandeja.Dispose();
            this.iconoDeBandeja = null;
        }

        private void ManejarCambioDeEstado(object remitente, EventArgs argumentos)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();

                if (this.iconoDeBandeja != null)
                {
                    this.iconoDeBandeja.ShowBalloonTip(2000);
                }
            }
            else
            {
                this.estadoDeLaVentana = WindowState;
            }
        }

        private void ManejarCambioDeVisibilidad(object remitente, DependencyPropertyChangedEventArgs argumentos)
        {
            if (this.iconoDeBandeja != null)
            {
                this.iconoDeBandeja.Visible = !IsVisible;
            }
        }

        private void ClicSobreIcono(object remitente, EventArgs argumentos)
        {
            Show();
            WindowState = this.estadoDeLaVentana;
        }

        #endregion
    }
}
