using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand
using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // DataTable
using System.Windows;                           // MessageBox
using System.Windows.Input;                     // ICommand
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

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            Tablas = new List<TablaMapeada>();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
        }

        public SincronizacionViewModel(List<NodoViewModel> Nodos)
        {
            Tablas = new List<TablaMapeada>();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();

            foreach (NodoViewModel Nodo in Nodos)
            {
                /*
                 * Creamos una TablaMapeada por cada Tabla listada en el servidor remoto.
                 * Posteriormente se agregaran MapeoDeColumnas a estas TablaMapeada.
                 */
                Tablas.Add(Nodo.CrearTablaMapeada());
            }
        }

        #endregion

        #region Propiedades

        public List<TablaMapeada> Tablas { get; private set; }

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
            get { return _ListoOrden ?? (_ListoOrden = new RelayCommand(() => Listo = true)); }
        }

        #endregion

        #region Funciones

        private void AsociarAccion(object[] Argumento)
        {
            try
            {
                NodoViewModel NodoOrigen = Argumento[0] as NodoViewModel;
                NodoViewModel NodoDestino = Argumento[1] as NodoViewModel;

                NodoDestino.AsociarCon(NodoOrigen);
                ActualizarTabla(NodoDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DesasociarAccion(object Argumento)
        {
            try
            {
                NodoViewModel NodoDestino = Argumento as NodoViewModel;

                NodoDestino.Desasociarse();
                ActualizarTabla(NodoDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SincronizacionListaAccion()
        {/*
            TablasAEnviar.Clear();
            TablasAEnviar.Tables.Clear();

            foreach (DataTable T in _CacheDeTablas.Values)
            {
                TablasAEnviar.Tables.Add(T);
            }
            */
            try
            {
                /*
                // Aqui enviamos las tablas al servidor al otro lado del mundo... muajajajaja
                ServidorValeria.ValeriaClient Cliente = new ServidorValeria.ValeriaClient();
                Cliente.EnviarTablas(new ServidorValeria.DataSetXML()
                {
                    EsquemaXML = _TablasAEnviar.GetXmlSchema(),
                    XML = _TablasAEnviar.GetXml()
                });
                */
                Listo = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        private void ActualizarTabla(NodoViewModel Nodo)
        {
            Nodo.Explorador.NodoTablaActual = Nodo.Padre;
            Nodo.Explorador.TablaActual = CrearTabla(Nodo.MapaColumna.TablaPadre);
            _CacheDeTablas[Nodo.Padre] = Nodo.Explorador.TablaActual;
        }

        public DataTable CrearTabla(TablaMapeada Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            DataTable TempTablaMapeada = new DataTable(Tabla.Nombre);

            foreach (MapeoDeColumnas MapaCol in Tabla.MapasColumnas)
            {
                DataColumn TablaColSinTipo = new DataColumn(MapaCol.ColumnaDestino.Nombre);
                TempTablaMapeada.Columns.Add(TablaColSinTipo);

                if (MapaCol.ColumnaOrigen != null)
                {
                    NodoViewModel NodoCol = MapaCol.ColumnaOrigen.BuscarEnRepositorio();
                    DataTable Temp = NodoCol.Explorador.ObtenerTabla(NodoCol.Padre);
                    DataColumn TempCol = Temp.Columns[MapaCol.ColumnaOrigen.Nombre];

                    TempTablaMapeada.Columns.Remove(MapaCol.ColumnaDestino.Nombre);

                    DataColumn TablaColConTipo = new DataColumn(MapaCol.ColumnaDestino.Nombre, TempCol.DataType);
                    TempTablaMapeada.Columns.Add(TablaColConTipo);

                    while (TempTablaMapeada.Rows.Count < Temp.Rows.Count)
                    {
                        TempTablaMapeada.Rows.Add(TempTablaMapeada.NewRow());
                    }

                    for (int i = 0; i < Temp.Rows.Count; i++)
                    {
                        TempTablaMapeada.Rows[i][TablaColConTipo.ColumnName] = Temp.Rows[i][TempCol.ColumnName];
                    }
                }
            }

            return TempTablaMapeada;
        }

        public void Sincronizar(ObservableCollection<NodoViewModel> NodosLocales, ObservableCollection<NodoViewModel> NodosRemotos, List<string[]> Mapas)
        {
            NodoViewModel NodoOrigen = null;
            NodoViewModel NodoDestino = null;
            
            _CacheDeTablas.Clear();

            foreach (string[] Mapa in Mapas)
            {
                NodoOrigen = NodoViewModelExtensiones.RutaANodo(Mapa[0], NodosLocales);
                NodoDestino = NodoViewModelExtensiones.RutaANodo(Mapa[1], NodosRemotos);

                NodoDestino.AsociarCon(NodoOrigen);
                ActualizarTabla(NodoDestino);
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
            
            foreach (TablaMapeada TM in Tablas)
            {
                foreach (MapeoDeColumnas MC in TM.MapasColumnas)
                {
                    if (MC.ColumnaOrigen == null)
                        continue;
                    
                    RutaNodoOrigen = MC.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta();
                    NodoOrigen = NodoViewModelExtensiones.RutaANodo(RutaNodoOrigen, NodosLocales);
                    
                    NodoDestino = MC.ColumnaDestino.BuscarEnRepositorio();
                    NodoDestino.AsociarCon(NodoOrigen);
                }

                ActualizarTabla(NodoDestino);
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
