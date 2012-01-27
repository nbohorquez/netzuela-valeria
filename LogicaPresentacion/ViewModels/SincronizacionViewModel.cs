using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand
using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // DataTable
using System.Windows;                           // MessageBox
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes
using Zuliaworks.Netzuela.Valeria.Logica;       // TablaMapeada

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class SincronizacionViewModel : ObservableObject
    {
        #region Variables

        private Dictionary<NodoViewModel, DataTable> _CacheDeTablas;
        private RelayCommand<object[]> _AsociarOrden;
        private RelayCommand<object> _DesasociarOrden;
        private RelayCommand _ListoOrden;
        private bool _Listo;
        private bool _PermitirModificaciones;

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            Tablas = new List<TablaDeAsociaciones>();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
        }

        public SincronizacionViewModel(List<NodoViewModel> Nodos)
            : base()
        {
            foreach (NodoViewModel Nodo in Nodos)
            {
                /*
                 * Creamos una TablaMapeada por cada Tabla listada en el servidor remoto.
                 * Posteriormente se agregaran MapeoDeColumnas a estas TablaMapeada.
                 */
                Tablas.Add(Nodo.CrearTablaDeAsociaciones());
            }
        }

        #endregion

        #region Propiedades

        public List<TablaDeAsociaciones> Tablas { get; private set; }

        public bool Listo
        {
            get { return _Listo; }
            private set
            {
                if (value != _Listo)
                {
                    _Listo = value;
                    RaisePropertyChanged("Listo");
                }
            }
        }
        
        public string BotonSincronizar
        {
            get { return (Listo == true) ? "Desincronizar" : "Sincronizar"; }
        }

        public bool PermitirModificaciones
        {
            get { return (Listo == true) ? false : true; }
        }

        public ICommand AsociarOrden
        {
            get { return _AsociarOrden ?? (_AsociarOrden = new RelayCommand<object[]>(param => this.AsociarAccion(param))); }
        }

        public ICommand DesasociarOrden
        {
            get { return _DesasociarOrden ?? (_DesasociarOrden = new RelayCommand<object>(param => this.DesasociarAccion(param))); }
        }

        public ICommand ListoOrden
        {
            get { return _ListoOrden ?? (_ListoOrden = new RelayCommand(() => ListoAccion())); }
        }

        #endregion

        #region Funciones

        private void AsociarAccion(object[] Argumento)
        {
            try
            {
                NodoViewModel NodoOrigen = Argumento[0] as NodoViewModel;
                NodoViewModel NodoDestino = Argumento[1] as NodoViewModel;

                Asociar(NodoOrigen, NodoDestino);
                //ActualizarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
                IntegrarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void DesasociarAccion(object Argumento)
        {
            try
            {
                NodoViewModel NodoDestino = Argumento as NodoViewModel;
                
                Desasociar(NodoDestino);
                //ActualizarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
                IntegrarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ListoAccion()
        {
            try
            {
                Listo = !Listo;
                RaisePropertyChanged("BotonSincronizar");
                RaisePropertyChanged("PermitirModificaciones");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void Asociar(NodoViewModel NodoOrigen, NodoViewModel NodoDestino)
        {
            if (NodoOrigen == null)
                throw new ArgumentNullException("NodoOrigen");
            if (NodoDestino == null)
                throw new ArgumentNullException("NodoDestino");

            DataTable TablaOrigen = NodoOrigen.Explorador.ObtenerTablaDeCache(NodoOrigen.Padre);
            DataTable TablaDestino = NodoDestino.Explorador.ObtenerTablaDeCache(NodoDestino.Padre);

            System.Type TipoOrigen = TablaOrigen.Columns[NodoOrigen.Nombre].DataType;
            System.Type TipoDestino = TablaDestino.Columns[NodoDestino.Nombre].DataType;

            if (TipoOrigen != TipoDestino)
            {
                throw new Exception("Los tipos de datos de ambas columnas tienen que ser iguales. "
                                    + TipoOrigen.ToString() + " != " + TipoDestino.ToString());
            }

            // Si es la primera vez que asociamos un nodo de esta tabla, agregamos a Tablas
            // una nueva AsociacionDeColumnas cuya ColumnaDestino sea NodoDestino.
            if (NodoDestino.Padre.TablaDeSocios == null)
                Tablas.Add(NodoDestino.Padre.CrearTablaDeAsociaciones());

            // Agregamos la DataTable a la cache local de tablas si ya no esta agregada
            if (!_CacheDeTablas.ContainsKey(NodoDestino.Padre))
                _CacheDeTablas[NodoDestino.Padre] = TablaDestino;

            NodoDestino.AsociarCon(NodoOrigen);
            
            //ActualizarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
        }

        private void Desasociar(NodoViewModel NodoDestino)
        {
            if (NodoDestino == null)
                throw new ArgumentNullException("NodoDestino");

            NodoDestino.Desasociarse();

            //ActualizarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
        }

        private void ActualizarTabla(DataTable Tabla, TablaDeAsociaciones TabAso)
        {
            // Como borrar la referencia del DataGrid a un DataTable:
            // http://social.msdn.microsoft.com/Forums/en/wpf/thread/a5767cf4-8d26-4f72-b1b1-feca26bb6b2e

            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (TabAso == null)
                throw new ArgumentNullException("TabAso");

            DataTable Temp = CrearTabla(TabAso);
            Tabla.Merge(Temp, false, MissingSchemaAction.Error);
            Temp.Dispose();
            Temp = null;
        }

        public void ActualizarTodasLasTablas()
        {
            try
            {
                foreach (TablaDeAsociaciones TM in Tablas)
                {
                    ActualizarTabla(_CacheDeTablas[TM.NodoTabla.BuscarEnRepositorio()], TM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar las tablas", ex);
            }
        }

        public void IntegrarTabla(DataTable Tabla, TablaDeAsociaciones TabAso)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (TabAso == null)
                throw new ArgumentNullException("TabAso");

            foreach (AsociacionDeColumnas Sociedad in TabAso.Sociedades)
            {
                if (Sociedad.ColumnaOrigen != null)
                {
                    NodoViewModel ColumnaOrigen = Sociedad.ColumnaOrigen.BuscarEnRepositorio();
                    NodoViewModel ColumnaDestino = Sociedad.ColumnaDestino.BuscarEnRepositorio();

                    DataTable TablaOrigen = ColumnaOrigen.Explorador.ObtenerTablaDeCache(ColumnaOrigen.Padre);

                    for (int i = 0; i < TablaOrigen.Rows.Count; i++)
                    {
                        if (Tabla.Rows.Count < TablaOrigen.Rows.Count)
                        {
                            DataRow FilaNueva = Tabla.NewRow();
                            FilaNueva[ColumnaDestino.Nombre] = TablaOrigen.Rows[i][ColumnaOrigen.Nombre];
                            Tabla.Rows.Add(FilaNueva);
                        }
                        else
                        {
                            Tabla.Rows[i][ColumnaDestino.Nombre] = TablaOrigen.Rows[i][ColumnaOrigen.Nombre];
                        }
                    }
                }
            }

            Tabla.AcceptChanges();
        }

        public DataTable CrearTabla(TablaDeAsociaciones Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            NodoViewModel NodoTablaDestino = Tabla.NodoTabla.BuscarEnRepositorio();
            DataTable TablaDestino = NodoTablaDestino.Explorador.ObtenerTablaDeCache(NodoTablaDestino);
            DataTable Resultado = new DataTable(Tabla.NodoTabla.Nombre);

            NodoTablaDestino = null;

            try
            {
                foreach (AsociacionDeColumnas Sociedad in Tabla.Sociedades)
                {
                    DataColumn ColDestino = new DataColumn(Sociedad.ColumnaDestino.Nombre, TablaDestino.Columns[Sociedad.ColumnaDestino.Nombre].DataType);
                    Resultado.Columns.Add(ColDestino);

                    ColDestino = null;
                    
                    if (Sociedad.ColumnaOrigen != null)
                    {
                        NodoViewModel NodoColOrigen = Sociedad.ColumnaOrigen.BuscarEnRepositorio();
                        DataTable TablaOrigen = NodoColOrigen.Explorador.ObtenerTablaDeCache(NodoColOrigen.Padre);
                        
                        for (int i = 0; i < TablaOrigen.Rows.Count; i++)
                        {
                            if (Resultado.Rows.Count < TablaOrigen.Rows.Count)
                            {
                                Resultado.Rows.Add(Resultado.NewRow());
                            }

                            Resultado.Rows[i][Sociedad.ColumnaDestino.Nombre] = TablaOrigen.Rows[i][Sociedad.ColumnaOrigen.Nombre];
                        }

                        NodoColOrigen = null;
                        TablaOrigen = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la tabla a partir de la TablaDeAsociaciones", ex);
            }

            Resultado.AcceptChanges();

            return Resultado;
        }

        public void Sincronizar(ObservableCollection<NodoViewModel> NodosLocales, ObservableCollection<NodoViewModel> NodosRemotos, List<string[]> Mapas)
        {
            NodoViewModel NodoOrigen = null;
            NodoViewModel NodoDestino = null;
            
            _CacheDeTablas.Clear();

            try
            {
                foreach (string[] Mapa in Mapas)
                {
                    NodoOrigen = NodoViewModelExtensiones.RutaANodo(Mapa[0], NodosLocales);
                    NodoDestino = NodoViewModelExtensiones.RutaANodo(Mapa[1], NodosRemotos);

                    Asociar(NodoOrigen, NodoDestino);
                    //ActualizarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
                    IntegrarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al sincronizar los arboles de nodos", ex);
            }
        }

        /// <summary>
        /// Vuelve a vincular las columnas de destino con las de origen. Se emplea generalmente 
        /// cuando se actualizan las tablas de origen desde el servidor local.
        /// </summary>
        /// <param name="NodosLocales">Es la coleccion de nodos que contiene las columnas de origen nuevas</param>
        public void RecargarTablasLocales(ObservableCollection<NodoViewModel> NodosLocales)
        {
            string RutaNodoOrigen = string.Empty;
            NodoViewModel NodoDestino = null;
            NodoViewModel NodoOrigen = null;

            _CacheDeTablas.Clear();

            try
            {
                foreach (TablaDeAsociaciones TM in Tablas)
                {
                    foreach (AsociacionDeColumnas MC in TM.Sociedades)
                    {
                        if (MC.ColumnaOrigen == null)
                            continue;

                        RutaNodoOrigen = MC.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta();
                        NodoOrigen = NodoViewModelExtensiones.RutaANodo(RutaNodoOrigen, NodosLocales);
                        NodoDestino = MC.ColumnaDestino.BuscarEnRepositorio();

                        Asociar(NodoOrigen, NodoDestino);
                    }

                    //ActualizarTabla(_CacheDeTablas[TM.NodoTabla.BuscarEnRepositorio()], TM);
                    IntegrarTabla(_CacheDeTablas[TM.NodoTabla.BuscarEnRepositorio()], TM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recargar las tablas locales", ex);
            }
        }

        public Dictionary<NodoViewModel, DataTable> TablasAEnviar()
        {
            // Creamos una copia solamente. De esta forma, las modificaciones externas no afectaran 
            // al objeto interno
            //return new Dictionary<NodoViewModel, DataTable>(_CacheDeTablas);
            return _CacheDeTablas;
        }

        public List<NodoViewModel> ObtenerNodosCache()
        {
            return _CacheDeTablas.Keys.ToList();
        }

        public List<DataTable> ObtenerTablasCache()
        {
            return _CacheDeTablas.Values.ToList();
        }

        public string[] RutasDeNodosDeOrigen()
        {
            List<string> NodosOrigen = new List<string>();

            try
            {
                foreach (NodoViewModel Nodo in NodosDeOrigen())
                {
                    NodosOrigen.Add(Nodo.RutaCompleta());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las rutas de los nodos de origen", ex);
            }

            return NodosOrigen.ToArray();
        }

        public string[] RutasDeNodosDeDestino()
        {
            List<string> NodosDestino = new List<string>();
            
            try
            {
                foreach (NodoViewModel Nodo in NodosDeDestino())
                {
                    NodosDestino.Add(Nodo.RutaCompleta());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las rutas de los nodos de destino", ex);
            }

            return NodosDestino.ToArray();
        }

        public NodoViewModel[] NodosDeOrigen()
        {
            List<NodoViewModel> NodosOrigen = new List<NodoViewModel>();

            try
            {
                // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
                foreach (TablaDeAsociaciones TM in Tablas)
                {
                    foreach (AsociacionDeColumnas MC in TM.Sociedades)
                    {
                        if (MC.ColumnaOrigen != null)
                            NodosOrigen.Add(MC.ColumnaOrigen.BuscarEnRepositorio());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los nodos de origen", ex);
            }

            return NodosOrigen.ToArray();
        }

        public NodoViewModel[] NodosDeDestino()
        {
            List<NodoViewModel> NodosDestino = new List<NodoViewModel>();

            try
            {
                // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
                foreach (TablaDeAsociaciones TM in Tablas)
                {
                    foreach (AsociacionDeColumnas MC in TM.Sociedades)
                    {
                        if (MC.ColumnaDestino != null)
                            NodosDestino.Add(MC.ColumnaDestino.BuscarEnRepositorio());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los nodos de destino", ex);
            }

            return NodosDestino.ToArray();
        }

        #endregion
    }
}
