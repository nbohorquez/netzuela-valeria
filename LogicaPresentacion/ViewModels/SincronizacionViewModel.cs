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
        {
            Tablas = new List<TablaDeAsociaciones>();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();

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
                    RaisePropertyChanged("BotonSincronizar");
                    RaisePropertyChanged("PermitirModificaciones");
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
            get { return _ListoOrden ?? (_ListoOrden = new RelayCommand(() => Listo = !Listo)); }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void SincronizacionListaAccion()
        {
            try
            {
                Listo = true;
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
            
            if (NodoDestino.Padre.TablaDeSocios == null)
            {
                Tablas.Add(NodoDestino.Padre.CrearTablaDeAsociaciones());
            }

            NodoDestino.AsociarCon(NodoOrigen);

            //ActualizarTabla(NodoDestino.Padre);
            ActualizarTabla(NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
            _CacheDeTablas[NodoDestino.Padre] = NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre];
        }

        private void Desasociar(NodoViewModel NodoDestino)
        {
            if (NodoDestino == null)
                throw new ArgumentNullException("NodoDestino");

            NodoDestino.Desasociarse();
            //ActualizarTabla(NodoDestino.Padre);
            ActualizarTabla(NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
            _CacheDeTablas[NodoDestino.Padre] = NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre];
        }

        private void ActualizarTabla(NodoViewModel Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");
            if (Nodo.Nivel != Constantes.NivelDeNodo.TABLA)
                throw new ArgumentException("El nodo tiene que ser de nivel Tabla");

            // Como borrar la referencia del DataGrid a un DataTable:
            // http://social.msdn.microsoft.com/Forums/en/wpf/thread/a5767cf4-8d26-4f72-b1b1-feca26bb6b2e


            DataTable T = Nodo.Explorador.TablaActual;
            Nodo.Explorador.TablaActual = CrearTabla(Nodo.Sociedad.TablaPadre);

            if (_CacheDeTablas.ContainsKey(Nodo.Padre))
                _CacheDeTablas.Remove(Nodo.Padre);

            T.Clear();
            T.Columns.Clear();
            T.DefaultView.Dispose();
            T.Dispose();
            T = null;
            
            _CacheDeTablas[Nodo.Padre] = Nodo.Explorador.TablaActual;             
        }

        private void ActualizarTabla(DataTable Tabla, TablaDeAsociaciones TabAso)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");
            if (TabAso == null)
                throw new ArgumentNullException("TabMap");

            foreach (AsociacionDeColumnas Sociedad in TabAso.Sociedades)
            {
                if (Sociedad.ColumnaOrigen != null)
                {
                    NodoViewModel ColumnaOrigen = Sociedad.ColumnaOrigen.BuscarEnRepositorio();
                    NodoViewModel ColumnaDestino = Sociedad.ColumnaDestino.BuscarEnRepositorio();
                    DataTable Temp = ColumnaOrigen.Explorador.ObtenerTablaDeCache(ColumnaOrigen.Padre);

                    System.Type TipoOrigen = Temp.Columns[ColumnaOrigen.Nombre].DataType;
                    System.Type TipoDestino = Tabla.Columns[ColumnaDestino.Nombre].DataType;

                    if (TipoOrigen != TipoDestino)
                        throw new Exception("Los tipos de datos de ambas columnas tienen que ser iguales. " 
                                            + TipoOrigen.ToString() + " != " + TipoDestino.ToString());

                    for (int i = 0; i < Temp.Rows.Count; i++)
                    {
                        if (Tabla.Rows.Count < Temp.Rows.Count)
                            Tabla.Rows.Add(Tabla.NewRow());

                        Tabla.Rows[i][ColumnaDestino.Nombre] = Temp.Rows[i][ColumnaOrigen.Nombre];
                    }
                }
            }
        }

        public DataTable CrearTabla(TablaDeAsociaciones Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            DataTable TempTablaAsoc = new DataTable(Tabla.NodoTabla.Nombre);

            try
            {
                foreach (AsociacionDeColumnas Sociedad in Tabla.Sociedades)
                {
                    DataColumn TablaColSinTipo = new DataColumn(Sociedad.ColumnaDestino.Nombre);
                    TempTablaAsoc.Columns.Add(TablaColSinTipo);

                    if (Sociedad.ColumnaOrigen != null)
                    {
                        NodoViewModel NodoCol = Sociedad.ColumnaOrigen.BuscarEnRepositorio();

                        DataTable Temp = NodoCol.Explorador.ObtenerTablaDeCache(NodoCol.Padre);
                        DataColumn TempCol = Temp.Columns[Sociedad.ColumnaOrigen.Nombre];
                        TempTablaAsoc.Columns.Remove(Sociedad.ColumnaDestino.Nombre);

                        DataColumn TablaColConTipo = new DataColumn(Sociedad.ColumnaDestino.Nombre, TempCol.DataType);
                        TempTablaAsoc.Columns.Add(TablaColConTipo);

                        TablaColSinTipo.Dispose();

                        for (int i = 0; i < Temp.Rows.Count; i++)
                        {
                            if (TempTablaAsoc.Rows.Count < Temp.Rows.Count)
                            {
                                TempTablaAsoc.Rows.Add(TempTablaAsoc.NewRow());
                            }

                            TempTablaAsoc.Rows[i][TablaColConTipo.ColumnName] = Temp.Rows[i][TempCol.ColumnName];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la tabla a partir de la TablaMapeada", ex);
            }

            return TempTablaAsoc;
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
                        NodoDestino.AsociarCon(NodoOrigen);
                    }

                    //ActualizarTabla(NodoDestino);
                    ActualizarTabla(NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
                    _CacheDeTablas[NodoDestino.Padre] = NodoDestino.Explorador.CacheDeTablas[NodoDestino.Padre];
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
            return new Dictionary<NodoViewModel, DataTable>(_CacheDeTablas);
        }

        #endregion
    }
}
