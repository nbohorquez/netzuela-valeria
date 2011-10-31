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

using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Comunes;      // ConvertirAObservableCollection
using Zuliaworks.Netzuela.Valeria.Datos;        // ServidorLocal
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /// <summary>
    /// Muestra al usuario las opciones de conexion de cada servidor detectado
    /// y le permite seleccionar una de ellas.
    /// </summary>
    public partial class VentanaDetectarServidoresLocales : Window
    {
        #region Variables
        
        private Conexion Conexion;

        #endregion

        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Conexion"></param>
        public VentanaDetectarServidoresLocales(ref Conexion Conexion)
        {
            InitializeComponent();

            this.Conexion = Conexion;
            this.ServidoresDetectados = Conexion.DetectarServidoresLocales().ConvertirAObservableCollection();

            cmb_Servidor.ItemsSource = this.ServidoresDetectados;
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; set; }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void btn_Aceptar_Clic(object sender, RoutedEventArgs e)
        {
            Conexion.Datos.Anfitrion = "localhost";
            Conexion.Datos.Servidor = cmb_Servidor.SelectedValue != null ? cmb_Servidor.SelectedValue.ToString() : "";
            Conexion.Datos.Instancia = cmb_Instancia.SelectedValue != null ? cmb_Instancia.SelectedValue.ToString() : "";
            Conexion.Datos.MetodoDeConexion = cmb_MetodoDeConexion.SelectedValue != null ? cmb_MetodoDeConexion.SelectedValue.ToString() : "";
            Conexion.Datos.ArgumentoDeConexion = cmb_ArgumentoDeConexion.SelectedValue != null ? cmb_ArgumentoDeConexion.SelectedValue.ToString() : "";

            this.Close();
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
