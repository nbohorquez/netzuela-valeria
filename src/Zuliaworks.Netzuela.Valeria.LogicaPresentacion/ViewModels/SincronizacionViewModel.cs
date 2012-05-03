﻿namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using MvvmFoundation.Wpf;                               // RelayCommand

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;                   // ObservableCollection
    using System.Data;                                      // DataTable
    using System.Linq;
    using System.Text;
    using System.Windows;                                   // MessageBox
    using System.Windows.Input;                             // ICommand
    
    using Zuliaworks.Netzuela.Valeria.Comunes;              // Constantes
    using Zuliaworks.Netzuela.Valeria.Logica;               // TablaMapeada
    using Zuliaworks.Netzuela.Valeria.LogicaPresentacion;   // ManipuladorDeTablas

    /// <summary>
    /// 
    /// </summary>
    public class SincronizacionViewModel : ObservableObject, IDisposable
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

        ~SincronizacionViewModel()
        {
            Dispose(false);
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
                NodoViewModel NodoOrigen = (NodoViewModel)Argumento[0];
                NodoViewModel NodoDestino = (NodoViewModel)Argumento[1];

                Asociar(NodoOrigen, NodoDestino);
                ManipuladorDeTablas.IntegrarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
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
                NodoViewModel NodoDestino = (NodoViewModel)Argumento;
                
                Desasociar(NodoDestino);
                ManipuladorDeTablas.IntegrarTabla(_CacheDeTablas[NodoDestino.Padre], NodoDestino.Sociedad.TablaPadre);
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
                ConmutarListo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ConmutarListo()
        {
            Listo = !Listo;
            RaisePropertyChanged("BotonSincronizar");
            RaisePropertyChanged("PermitirModificaciones");
        }

        private void ListoTrue()
        {
            if (Listo == false)
                ConmutarListo();
        }

        private void ListoFalse()
        {
            if (Listo == true)
                ConmutarListo();
        }

        private void Asociar(NodoViewModel NodoOrigen, NodoViewModel NodoDestino)
        {
            if (NodoOrigen == null)
                throw new ArgumentNullException("NodoOrigen");
            if (NodoDestino == null)
                throw new ArgumentNullException("NodoDestino");

            DataTable TablaOrigen = NodoOrigen.Explorador.ObtenerTablaDeCache(NodoOrigen.Padre);
            DataTable TablaDestino = NodoDestino.Explorador.ObtenerTablaDeCache(NodoDestino.Padre);

            Type TipoOrigen = TablaOrigen.Columns[NodoOrigen.Nombre].DataType;
            Type TipoDestino = TablaDestino.Columns[NodoDestino.Nombre].DataType;

            if (TipoOrigen != TipoDestino)
            {
                throw new Exception("Los tipos de datos de ambas columnas tienen que ser iguales. "
                                    + TipoOrigen.ToString() + " != " + TipoDestino.ToString());
            }

            /*
             * Si es la primera vez que asociamos un nodo de esta tabla, agregamos a Tablas
             * una nueva AsociacionDeColumnas cuya ColumnaDestino sea NodoDestino.
             */
            if (NodoDestino.Padre.TablaDeSocios == null)
            {
                Tablas.Add(NodoDestino.Padre.CrearTablaDeAsociaciones());
            }

            // Agregamos la DataTable a la cache local de tablas si ya no esta agregada
            if (!_CacheDeTablas.ContainsKey(NodoDestino.Padre))
            {
                _CacheDeTablas[NodoDestino.Padre] = TablaDestino;
            }

            NodoDestino.AsociarCon(NodoOrigen);
        }

        private void Desasociar(NodoViewModel NodoDestino)
        {
            if (NodoDestino == null)
                throw new ArgumentNullException("NodoDestino");

            NodoDestino.Desasociarse();
        }

        private void BorrarCacheDeTablas()
        {
            DataTable[] tablas = _CacheDeTablas.Values.ToArray();

            for (int i = 0; i < tablas.Length; i++)
            {
                tablas[i].Dispose();
                tablas[i] = null;
            }

            tablas = null;
            _CacheDeTablas.Clear();
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            _AsociarOrden = null;
            _DesasociarOrden = null;
            _ListoOrden = null;
            _Listo = false;
            _PermitirModificaciones = false;

            if (borrarCodigoAdministrado)
            {
                if(_CacheDeTablas != null)
                {
                    _CacheDeTablas.Clear();
                    _CacheDeTablas = null;
                }

                if (Tablas != null)
                {
                    for (int i = 0; i < Tablas.Count; i++)
                    {
                        Tablas[i].Dispose();
                    }

                    Tablas.Clear();
                    Tablas = null;
                }
            }
        }

        public void ActualizarTodasLasTablas()
        {
            try
            {
                foreach (TablaDeAsociaciones TM in Tablas)
                {
                     ManipuladorDeTablas.ActualizarTabla(_CacheDeTablas[TM.NodoTabla.BuscarEnRepositorio()], TM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar las tablas", ex);
            }
        }
        
        public void Sincronizar(ObservableCollection<NodoViewModel> NodosLocales, ObservableCollection<NodoViewModel> NodosRemotos, List<string[]> Asociaciones)
        {
            NodoViewModel NodoOrigen = null;
            NodoViewModel NodoDestino = null;

            BorrarCacheDeTablas();

            try
            {
                foreach (string[] Asociacion in Asociaciones)
                {
                    NodoOrigen = NodoViewModelExtensiones.RutaANodo(Asociacion[0], NodosLocales);
                    NodoDestino = NodoViewModelExtensiones.RutaANodo(Asociacion[1], NodosRemotos);

                    Asociar(NodoOrigen, NodoDestino);
                }

                ListoTrue();
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

            BorrarCacheDeTablas();

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
            //return _CacheDeTablas;
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
                // Obtenemos las columnas de destino que son utilizadas en la sincronizacion
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

        /// <summary>
        /// Lee la tabla especificada desde el proveedor de datos.
        /// </summary>
        /// <param name="Tabla">Nodo del árbol de datos cuya tabla se quiere obtener</param>
        /// <returns>Tabla leída desde el proveedor de datos o nulo si no se pudo encontrar.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="Tabla"/> es una referencia 
        /// nula.</exception>
        public DataTable ObtenerTablaDeCache(NodoViewModel Tabla)
        {
            DataTable Resultado = null;

            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            try
            {
                Resultado = _CacheDeTablas[Tabla];
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener tabla de la cache", ex);
            }

            return Resultado;
        }

        public bool BorrarTablaDeCache(NodoViewModel Tabla)
        {
            bool resultado = false;

            if (Tabla == null)
            {
                throw new ArgumentNullException("Tabla");
            }

            try
            {
                resultado = _CacheDeTablas.Remove(Tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al borrar tabla de la cache", ex);
            }

            return resultado;
        }

        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
