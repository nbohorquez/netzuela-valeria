using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Logica;       // TablaMapeada

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class SincronizacionViewModel
    {
        #region Variables

        private List<TablaMapeada> Tablas;
        private RelayCommand<object> _AsociarOrden;
        private RelayCommand<object> _DesasociarOrden;

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            Tablas = new List<TablaMapeada>();
        }

        public SincronizacionViewModel(List<NodoViewModel> Nodos)
        {
            Tablas = new List<TablaMapeada>();

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

        public ICommand AsociarOrden
        {
            get { return _AsociarOrden ?? (_AsociarOrden = new RelayCommand<object>(param => this.Asociar(param))); }
        }

        public ICommand DesasociarOrden
        {
            get { return _DesasociarOrden ?? (_DesasociarOrden = new RelayCommand<object>(param => this.Desasociar(param))); }
        }

        #endregion

        #region Funciones

        private void Asociar(object Argumento)
        {
            NodoViewModel[] Nodos = Argumento as NodoViewModel[];

            NodoViewModel NodoLocalActual = Nodos[0] as NodoViewModel;
            NodoViewModel NodoRemotoActual = Nodos[1] as NodoViewModel;
            
            MapeoDeColumnas MapCol = NodoRemotoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Asociar(ArbolLocal.NodoActual);

                TablaMapeada MapTbl = MapCol.Tabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }
        }

        private void Desasociar(object Argumento)
        {
            MapeoDeColumnas MapCol = ArbolRemoto.NodoActual.MapaColumna;

            if (MapCol != null)
            {
                MapCol.Desasociar();

                TablaMapeada MapTbl = MapCol.Tabla;
                ArbolRemoto.TablaActual = MapTbl.TablaMapeada();
            }
        }

        #endregion
    }
}
