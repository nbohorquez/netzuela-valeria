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
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;

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
        /// <param name="ServidoresDetectados"></param>
        public VentanaDetectarServidoresLocales(ObservableCollection<ServidorLocal> ServidoresDetectados)
        {
            InitializeComponent();

            // Establezco el DataContext aqui porque necesito acceder a "CerrarView" desde la ventana.
            var ViewModel = new DetectarServidoresLocalesViewModel(ServidoresDetectados);
            this.DataContext = ViewModel;
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; set; }

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
