namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;                           // ObservableCollection
    using System.Data;                                              // DataTable
    using System.Linq;
    using System.Text;
    using System.Windows;                                           // MessageBox
    using System.Windows.Input;                                     // ICommand

    using MvvmFoundation.Wpf;                                       // RelayCommand
    using Zuliaworks.Netzuela.Valeria.Comunes;                      // Constantes
    using Zuliaworks.Netzuela.Valeria.Cliente.Logica;               // TablaMapeada
    using Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion;   // ManipuladorDeTablas

    /// <summary>
    /// 
    /// </summary>
    public class SincronizacionViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private RelayCommand<object[]> asociarOrden;
        private RelayCommand<object> desasociarOrden;
        private RelayCommand listoOrden;
        private bool listo;
        private bool permitirModificaciones;

        #endregion

        #region Constructores

        public SincronizacionViewModel()
        {
            this.Tablas = new List<TablaDeAsociaciones>();
        }

        public SincronizacionViewModel(List<NodoViewModel> nodos)
            : base()
        {
            foreach (NodoViewModel nodo in nodos)
            {
                /*
                 * Creamos una TablaMapeada por cada Tabla listada en el servidor remoto.
                 * Posteriormente se agregaran MapeoDeColumnas a estas TablaMapeada.
                 */
                this.Tablas.Add(nodo.CrearTablaDeAsociaciones());
            }
        }

        ~SincronizacionViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public List<TablaDeAsociaciones> Tablas { get; private set; }

        public bool Listo
        {
            get { return listo; }
            private set
            {
                if (value != listo)
                {
                    listo = value;
                    this.RaisePropertyChanged("Listo");
                }
            }
        }
        
        public string BotonSincronizar
        {
            get { return (this.Listo == true) ? "Desincronizar" : "Sincronizar"; }
        }

        public bool PermitirModificaciones
        {
            get { return (this.Listo == true) ? false : true; }
        }

        public ICommand AsociarOrden
        {
            get { return this.asociarOrden ?? (this.asociarOrden = new RelayCommand<object[]>(param => this.AsociarAccion(param))); }
        }

        public ICommand DesasociarOrden
        {
            get { return this.desasociarOrden ?? (this.desasociarOrden = new RelayCommand<object>(param => this.DesasociarAccion(param))); }
        }

        public ICommand ListoOrden
        {
            get { return this.listoOrden ?? (this.listoOrden = new RelayCommand(() => this.ListoAccion())); }
        }

        #endregion

        #region Funciones

        private void AsociarAccion(object[] argumento)
        {
            try
            {
                NodoViewModel nodoOrigen = (NodoViewModel)argumento[0];
                NodoViewModel nodoDestino = (NodoViewModel)argumento[1];

                Asociar(nodoOrigen, nodoDestino);
                ManipuladorDeTablas.IntegrarTabla(nodoDestino.Padre.BuscarEnRepositorioDeTablas(), nodoDestino.Sociedad.TablaPadre);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void DesasociarAccion(object argumento)
        {
            try
            {
                NodoViewModel nodoDestino = (NodoViewModel)argumento;

                this.Desasociar(nodoDestino);
                ManipuladorDeTablas.IntegrarTabla(nodoDestino.Padre.BuscarEnRepositorioDeTablas(), nodoDestino.Sociedad.TablaPadre);
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
                this.ConmutarListo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ConmutarListo()
        {
            this.Listo = !this.Listo;
            this.RaisePropertyChanged("BotonSincronizar");
            this.RaisePropertyChanged("PermitirModificaciones");
        }

        private void ListoTrue()
        {
            if (this.Listo == false)
            {
                this.ConmutarListo();
            }
        }

        private void ListoFalse()
        {
            if (this.Listo == true)
            {
                this.ConmutarListo();
            }
        }

        private void Asociar(NodoViewModel nodoOrigen, NodoViewModel nodoDestino)
        {
            if (nodoOrigen == null)
            {
                throw new ArgumentNullException("nodoOrigen");
            }

            if (nodoDestino == null)
            {
                throw new ArgumentNullException("nodoDestino");
            }

            DataTable tablaOrigen = nodoOrigen.Padre.BuscarEnRepositorioDeTablas();
            DataTable tablaDestino = nodoDestino.Padre.BuscarEnRepositorioDeTablas();

            Type tipoOrigen = tablaOrigen.Columns[nodoOrigen.Nombre].DataType;
            Type tipoDestino = tablaDestino.Columns[nodoDestino.Nombre].DataType;

            if (tipoOrigen != tipoDestino)
            {
                throw new Exception("Los tipos de datos de ambas columnas tienen que ser iguales. "
                                    + tipoOrigen.ToString() + " != " + tipoDestino.ToString());
            }

            /*
             * Si es la primera vez que asociamos un nodo de esta tabla, agregamos a Tablas
             * una nueva AsociacionDeColumnas cuya ColumnaDestino sea NodoDestino.
             */
            if (nodoDestino.Padre.TablaDeSocios == null)
            {
                Tablas.Add(nodoDestino.Padre.CrearTablaDeAsociaciones());
            }

            nodoDestino.AsociarCon(nodoOrigen);
        }

        private void Desasociar(NodoViewModel nodoDestino)
        {
            if (nodoDestino == null)
            {
                throw new ArgumentNullException("nodoDestino");
            }

            // Desasociamos el nodo origen, no el destino
            NodoViewModel nodoOrigen = nodoDestino.Sociedad.ColumnaOrigen.BuscarEnRepositorioDeNodos();
            nodoOrigen.Desasociarse();
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            this.asociarOrden = null;
            this.desasociarOrden = null;
            this.listoOrden = null;
            this.listo = false;
            this.permitirModificaciones = false;

            if (borrarCodigoAdministrado)
            {
                if (this.Tablas != null)
                {
                    for (int i = 0; i < this.Tablas.Count; i++)
                    {
                        this.Tablas[i].Dispose();
                    }

                    this.Tablas.Clear();
                    this.Tablas = null;
                }
            }
        }

        public void ActualizarTodasLasTablas()
        {
            try
            {
                foreach (TablaDeAsociaciones tm in this.Tablas)
                {
                    DataTable tabla = tm.NodoTabla.BuscarEnRepositorioDeNodos().BuscarEnRepositorioDeTablas();
                    ManipuladorDeTablas.ActualizarTabla(tabla, tm);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar las tablas", ex);
            }
        }
        
        public void Sincronizar(ObservableCollection<NodoViewModel> nodosLocales, ObservableCollection<NodoViewModel> nodosRemotos, List<string[]> asociaciones)
        {
            NodoViewModel nodoOrigen = null;
            NodoViewModel nodoDestino = null;

            try
            {
                foreach (string[] asociacion in asociaciones)
                {
                    nodoOrigen = NodoViewModelExtensiones.RutaANodo(asociacion[0], nodosLocales);
                    nodoDestino = NodoViewModelExtensiones.RutaANodo(asociacion[1], nodosRemotos);

                    this.Asociar(nodoOrigen, nodoDestino);
                }

                this.ListoTrue();
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
        /// <param name="nodosLocales">Es la coleccion de nodos que contiene las columnas de origen nuevas</param>
        public void RecargarTablasLocales(ObservableCollection<NodoViewModel> nodosLocales)
        {
            string rutaNodoOrigen = string.Empty;
            NodoViewModel nodoDestino = null;
            NodoViewModel nodoOrigen = null;

            try
            {
                foreach (TablaDeAsociaciones tm in this.Tablas)
                {
                    foreach (AsociacionDeColumnas mc in tm.Sociedades)
                    {
                        if (mc.ColumnaOrigen == null)
                        {
                            continue;
                        }

                        rutaNodoOrigen = mc.ColumnaOrigen.BuscarEnRepositorioDeNodos().RutaCompleta();
                        nodoOrigen = NodoViewModelExtensiones.RutaANodo(rutaNodoOrigen, nodosLocales);
                        nodoDestino = mc.ColumnaDestino.BuscarEnRepositorioDeNodos();

                        this.Asociar(nodoOrigen, nodoDestino);
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
            Dictionary<NodoViewModel, DataTable> resultado = new Dictionary<NodoViewModel, DataTable>();

            foreach (TablaDeAsociaciones ta in this.Tablas)
            {
                NodoViewModel nodoTabla = ta.NodoTabla.BuscarEnRepositorioDeNodos();
                resultado.Add(nodoTabla, nodoTabla.BuscarEnRepositorioDeTablas());
            }

            return resultado;
        }
        
        public string[] RutasDeNodosDeOrigen()
        {
            List<string> nodosOrigen = new List<string>();

            try
            {
                foreach (NodoViewModel nodo in this.NodosDeOrigen())
                {
                    nodosOrigen.Add(nodo.RutaCompleta());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las rutas de los nodos de origen", ex);
            }

            return nodosOrigen.ToArray();
        }

        public string[] RutasDeNodosDeDestino()
        {
            List<string> nodosDestino = new List<string>();
            
            try
            {
                foreach (NodoViewModel nodo in this.NodosDeDestino())
                {
                    nodosDestino.Add(nodo.RutaCompleta());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las rutas de los nodos de destino", ex);
            }

            return nodosDestino.ToArray();
        }

        public NodoViewModel[] NodosDeOrigen()
        {
            List<NodoViewModel> nodosOrigen = new List<NodoViewModel>();

            try
            {
                // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
                foreach (TablaDeAsociaciones tm in this.Tablas)
                {
                    foreach (AsociacionDeColumnas mc in tm.Sociedades)
                    {
                        if (mc.ColumnaOrigen != null)
                        {
                            nodosOrigen.Add(mc.ColumnaOrigen.BuscarEnRepositorioDeNodos());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los nodos de origen", ex);
            }

            return nodosOrigen.ToArray();
        }

        public NodoViewModel[] NodosDeDestino()
        {
            List<NodoViewModel> nodosDestino = new List<NodoViewModel>();

            try
            {
                // Obtenemos las columnas de destino que son utilizadas en la sincronizacion
                foreach (TablaDeAsociaciones tm in this.Tablas)
                {
                    foreach (AsociacionDeColumnas mc in tm.Sociedades)
                    {
                        if (mc.ColumnaDestino != null)
                        {
                            nodosDestino.Add(mc.ColumnaDestino.BuscarEnRepositorioDeNodos());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los nodos de destino", ex);
            }

            return nodosDestino.ToArray();
        }

        public List<string[]> Asociaciones()
        {
            List<string[]> resultado = new List<string[]>();

            try
            {
                foreach (TablaDeAsociaciones ta in Tablas)
                {
                    foreach (AsociacionDeColumnas ac in ta.Sociedades)
                    {
                        if (ac.ColumnaDestino != null && ac.ColumnaOrigen != null)
                        {
                            resultado.Add(new string[] 
                            {
                                ac.ColumnaOrigen.BuscarEnRepositorioDeNodos().RutaCompleta(),
                                ac.ColumnaDestino.BuscarEnRepositorioDeNodos().RutaCompleta()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las asociaciones de columnas", ex);
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
