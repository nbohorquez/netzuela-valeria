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
    public class SincronizacionViewModel
    {
        #region Variables

        private List<TablaMapeada> _Tablas;
        private RelayCommand<object[]> _AsociarOrden;
        private RelayCommand<object> _DesasociarOrden;

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            _Tablas = new List<TablaMapeada>();
        }

        public SincronizacionViewModel(List<NodoViewModel> Nodos)
        {
            _Tablas = new List<TablaMapeada>();

            foreach (NodoViewModel Nodo in Nodos)
            {
                /*
                 * Creamos una TablaMapeada por cada Tabla listada en el servidor remoto.
                 * Posteriormente se agregaran MapeoDeColumnas a estas TablaMapeada.
                 */

                TablaMapeada MapTab = Nodo.CrearTablaMapeada();                
                _Tablas.Add(MapTab);
            }
        }

        #endregion

        #region Propiedades

        public ICommand AsociarOrden
        {
            get { return _AsociarOrden ?? (_AsociarOrden = new RelayCommand<object[]>(param => this.Asociar(param))); }
        }

        public ICommand DesasociarOrden
        {
            get { return _DesasociarOrden ?? (_DesasociarOrden = new RelayCommand<object>(param => this.Desasociar(param))); }
        }

        #endregion

        #region Funciones

        private void Asociar(object[] Argumento)
        {
            var NodoLocalActual = (NodoViewModel)Argumento[0];
            var NodoRemotoActual = (NodoViewModel)Argumento[1];

            try
            {
                NodoRemotoActual.AsociarseCon(NodoLocalActual);
                NodoRemotoActual.Explorador.TablaActual = CrearTabla(NodoRemotoActual.MapaColumna.TablaPadre);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Desasociar(object Argumento)
        {
            NodoViewModel NodoRemotoActual = Argumento as NodoViewModel;

            try
            {
                NodoRemotoActual.Desasociarse();
                NodoRemotoActual.Explorador.TablaActual = CrearTabla(NodoRemotoActual.MapaColumna.TablaPadre);
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
            DataTable TempTablaMapeada = new DataTable();

            foreach (MapeoDeColumnas MapaCol in Tabla.MapasColumnas)
            {
                DataColumn TablaColSinTipo = new DataColumn(MapaCol.ColumnaDestino.Nombre);
                TempTablaMapeada.Columns.Add(TablaColSinTipo);

                if (MapaCol.ColumnaOrigen != null)
                {
                    //DataTable Temp = MapaCol.ColumnaOrigen.Explorador.ObtenerTabla(MapaCol.ColumnaOrigen.Padre);
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
