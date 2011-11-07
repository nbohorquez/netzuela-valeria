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

        private Explorador ArbolLocal, ArbolRemoto;
        private List<MapeoDeTablas> LocalARemota;

        #endregion

        #region Constructores

        public VentanaPrincipal()
        {
            InitializeComponent();

            LocalARemota = new List<MapeoDeTablas>();

            //grp_ConexionLocal.DataContext = Local;
            //bar_BarraDeEstado.DataContext = Barra;
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void btn_Conectar_Clic(object sender, RoutedEventArgs e)
        {/*
            try
            {
                Local.Conectar(Credenciales.txt_Usuario.Text.ConvertirASecureString(), Credenciales.pwd_Contasena.SecurePassword);

                // "Pegamos" el estado de la conexion a la barra de estado
                Barra.EstadoConexion = Local.BD.Estado;

                if (Local.BD.Estado == ConnectionState.Open)
                {
                    ObservableCollection<Nodo> NodosLocales = new ObservableCollection<Nodo>()
                    {
                        new Nodo(Local.Datos.Servidor + "(" + Local.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                    };

                    ArbolLocal = new Explorador(NodosLocales, Local.BD);

                    trv_ExploradorLocal.DataContext = ArbolLocal;
                    txt_ElementoLocal.DataContext = ArbolLocal;
                    dgr_TablaLocal.DataContext = ArbolLocal;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.InnerException);
            }

            try
            {
                Remota.Conectar(null, null);

                if (Remota.BD.Estado == ConnectionState.Open)
                {
                    ObservableCollection<Nodo> NodosRemotos = new ObservableCollection<Nodo>()
                    {
                        new Nodo(Remota.Datos.Servidor + "(" + Remota.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                    };

                    ArbolRemoto = new Explorador(NodosRemotos, Remota.BD);

                    // Leemos todas las tablas de todas las bases de datos del servidor remoto
                    ArbolRemoto.ExpandirTodo();

                    // Obtenemos todos los nodos que son indice de tablas en la cache
                    List<Nodo> NodosCache = ArbolRemoto.ObtenerNodosCache();

                    foreach (Nodo Tabla in NodosCache)
                    {
                        // Creamos un MapeoDeTablas por cada Tabla listada en el servidor remoto
                        // Luego se agregaran MapeoDeColumnas a estos MapeoDeTablas.
                        MapeoDeTablas MapTab = new MapeoDeTablas(Tabla);
                        LocalARemota.Add(MapTab);
                    }

                    trv_ExploradorRemoto.DataContext = ArbolRemoto;
                    txt_ElementoRemoto.DataContext = ArbolRemoto;
                    dgr_TablaRemota.DataContext = ArbolRemoto;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.InnerException);
            }*/
        }

        private void btn_Asociar_Clic(object sender, RoutedEventArgs e)
        {
            MapeoDeColumnas MapCol = ArbolRemoto.NodoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Asociar(ArbolLocal.NodoActual);

                MapeoDeTablas MapTbl = MapCol.MapaTabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }
        }

        private void btn_Desasociar_Clic(object sender, RoutedEventArgs e)
        {
            MapeoDeColumnas MapCol = ArbolRemoto.NodoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Desasociar();

                MapeoDeTablas MapTbl = MapCol.MapaTabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }
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
