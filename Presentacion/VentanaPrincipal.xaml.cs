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

using System.Collections.ObjectModel;    // ObservableCollection
using System.Data;                       // DataTable
using Zuliaworks.Netzuela.Valeria.Comunes;                   // DatosDeConexion, Constantes
using Zuliaworks.Netzuela.Valeria.Datos;                     // ServidorLocal
using Zuliaworks.Netzuela.Valeria.Logica;                    // Conexion, ColeccionDeNodos, TablaDeDatos, BarraDeEstado

namespace Zuliaworks.Netzuela.Valeria.Presentacion
{
    /// <summary>
    /// Ventana de inicio del programa.
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        #region Variables

        private BarraDeEstado Barra;
        private Conexion Local, Remota;
        private Explorador ArbolLocal, ArbolRemoto;
        private List<MapeoDeTablas> LocalARemota;

        #endregion

        #region Constructores

        public VentanaPrincipal()
        {
            InitializeComponent();

            Local = new Conexion();
            Remota = new Conexion(new DatosDeConexion() { Servidor = Constantes.SGBDR.NETZUELA, Instancia = "Isla Providencia" });
            LocalARemota = new List<MapeoDeTablas>();
            Barra = new BarraDeEstado();
            
            grp_ConexionLocal.DataContext = Local;
            bar_BarraDeEstado.DataContext = Barra;
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void btn_Detectar_Clic(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ServidorLocal> ServidoresDetectados = Conexion.DetectarServidoresLocales().ConvertirAObservableCollection();
            VentanaDetectarServidoresLocales Servidores = new VentanaDetectarServidoresLocales(ServidoresDetectados);
            Servidores.ShowDialog();
        }

        private void btn_Conectar_Clic(object sender, RoutedEventArgs e)
        {
            VentanaAutentificacion Credenciales = new VentanaAutentificacion();
            Credenciales.ShowDialog();
            /*
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

        private void btn_Desconectar_Clic(object sender, RoutedEventArgs e)
        {
            Local.Desconectar();
        }

        private void tvi_Item_Expandir(object sender, RoutedEventArgs e)
        {
            TreeViewItem Item = e.OriginalSource as TreeViewItem;
            if (Item == null)
                return;

            Item.IsExpanded = false;

            Nodo ItemNodo = Item.DataContext as Nodo;
            if (ItemNodo == null)
                return;

            TreeView TV = ArbolVisual.BusquedaHaciaArriba<TreeView>(Item as DependencyObject) as TreeView;
            Explorador Arbol = TV.DataContext as Explorador;

            Arbol.Expandir(ItemNodo);
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

        private void tvi_Item_ClicBotonIzqRaton(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.IsExpanded = true;
        }

        private void tvi_Item_MouseEncima(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Italic;
        }

        private void tvi_Item_MouseAfuera(object sender, MouseEventArgs e)
        {
            TreeViewItem Item = ArbolVisual.BusquedaHaciaArriba<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;

            if (Item != null)
                Item.FontStyle = System.Windows.FontStyles.Normal;
        }

        private void dgr_Tablas_ClicBotonIzqRaton(object sender, MouseButtonEventArgs e)
        {
            DataGridCell Celda = ArbolVisual.BusquedaHaciaArriba<DataGridCell>(e.OriginalSource as DependencyObject) as DataGridCell;

            if (Celda == null)
                return;

            DataGrid Grilla = ArbolVisual.BusquedaHaciaArriba<DataGrid>(Celda as DependencyObject) as DataGrid;

            // Hacemos que Arbol.NodoActual sea la columna seleccionada
            Explorador Arbol = Grilla.DataContext as Explorador;
            Arbol.NodoActual = Nodo.BuscarNodo(Celda.Column.Header as string, Arbol.NodoTablaActual.Hijos);
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
