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

using Zuliaworks.Netzuela.Valeria.Comunes;                          // DatosDeConexion, Constantes
using Zuliaworks.Netzuela.Valeria.Datos;                            // ServidorLocal
using Zuliaworks.Netzuela.Valeria.Logica;                           // Conexion, ColeccionDeNodos, TablaDeDatos, BarraDeEstado

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /// <summary>
    /// Ventana de inicio del programa.
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        #region Variables

        private List<MapeoDeTablas> LocalARemota;

        #endregion

        #region Constructores

        public VentanaPrincipal()
        {
            InitializeComponent();

            LocalARemota = new List<MapeoDeTablas>();
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void btn_Asociar_Clic(object sender, RoutedEventArgs e)
        {/*
            MapeoDeColumnas MapCol = ArbolRemoto.NodoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Asociar(ArbolLocal.NodoActual);

                MapeoDeTablas MapTbl = MapCol.MapaTabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }*/
        }

        private void btn_Desasociar_Clic(object sender, RoutedEventArgs e)
        {
      /*      MapeoDeColumnas MapCol = ArbolRemoto.NodoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Desasociar();

                MapeoDeTablas MapTbl = MapCol.MapaTabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }*/
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
