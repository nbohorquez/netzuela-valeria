using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand
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

        private DataSet _TablasAEnviar;
        private Dictionary<NodoViewModel, DataTable> _CacheDeTablas;
        private RelayCommand<object[]> _AsociarOrden;
        private RelayCommand<object> _DesasociarOrden;
        private RelayCommand _ListoOrden;
        private bool _Completado;

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            Tablas = new List<TablaMapeada>();
            _TablasAEnviar = new DataSet();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
        }

        public SincronizacionViewModel(List<NodoViewModel> Nodos)
        {
            Tablas = new List<TablaMapeada>();
            _TablasAEnviar = new DataSet();
            _CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();

            foreach (NodoViewModel Nodo in Nodos)
            {
                /*
                 * Creamos una TablaMapeada por cada Tabla listada en el servidor remoto.
                 * Posteriormente se agregaran MapeoDeColumnas a estas TablaMapeada.
                 */

                TablaMapeada MapTab = Nodo.CrearTablaMapeada();                
                Tablas.Add(MapTab);
            }
        }

        #endregion

        #region Propiedades

        public List<TablaMapeada> Tablas { get; private set; }

        public bool Completado
        {
            get { return _Completado; }
            private set
            {
                if (value != _Completado)
                {
                    _Completado = value;
                    RaisePropertyChanged("Completado");
                }
            }
        }

        public ICommand AsociarOrden
        {
            get { return _AsociarOrden ?? (_AsociarOrden = new RelayCommand<object[]>(param => this.Asociar(param))); }
        }

        public ICommand DesasociarOrden
        {
            get { return _DesasociarOrden ?? (_DesasociarOrden = new RelayCommand<object>(param => this.Desasociar(param))); }
        }

        public ICommand ListoOrden
        {
            get { return _ListoOrden ?? (_ListoOrden = new RelayCommand(this.Listo)); }
        }

        #endregion

        #region Funciones

        private void Asociar(object[] Argumento)
        {
            try
            {
                var NodoLocalActual = (NodoViewModel)Argumento[0];
                var NodoRemotoActual = (NodoViewModel)Argumento[1];

                NodoRemotoActual.AsociarseCon(NodoLocalActual);
                NodoRemotoActual.Explorador.TablaActual = CrearTabla(NodoRemotoActual.MapaColumna.TablaPadre);

                _CacheDeTablas[NodoRemotoActual.Padre] = NodoRemotoActual.Explorador.TablaActual;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Desasociar(object Argumento)
        {
            try
            {
                NodoViewModel NodoRemotoActual = Argumento as NodoViewModel;

                NodoRemotoActual.Desasociarse();
                NodoRemotoActual.Explorador.TablaActual = CrearTabla(NodoRemotoActual.MapaColumna.TablaPadre);

                _CacheDeTablas[NodoRemotoActual.Padre] = NodoRemotoActual.Explorador.TablaActual;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Listo()
        {
            try
            {
                foreach (DataTable T in _CacheDeTablas.Values)
                {
                    _TablasAEnviar.Tables.Add(T);
                }

                _TablasAEnviar.WriteXml("Millijigui.xml");

                Completado = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ActualizarTodasLasTablas()
        {
            /*
            foreach (TablaMapeada T in _Tablas)
            {
                T. CrearTabla(T);
            }*/
        }

        private DataTable CrearTabla(TablaMapeada Tabla)
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
        
        #endregion
    }
}
