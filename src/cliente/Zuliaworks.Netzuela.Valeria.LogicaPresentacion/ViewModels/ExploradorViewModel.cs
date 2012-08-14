namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using MvvmFoundation.Wpf;                       // ObservableObject

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Data;                              // DataTable
    using System.Linq;
    using System.Text;
    using System.Windows;                           // MessageBox
    using System.Windows.Input;                     // ICommand

    using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

    /// <summary>
    /// Esta clase se emplea para acceder a los datos de una fuente cuya estructura 
    /// sea un árbol de nodos.
    /// </summary>
    public partial class ExploradorViewModel : ObservableObject
    {
        #region Variables

        private ConexionViewModel conexion;
        private NodoViewModel nodoActual;
        private RelayCommand<NodoViewModel> expandirOrden;
        private RelayCommand<string> establecerNodoActualOrden;

        #endregion

        #region Constructores

        /// <summary>
        /// Construye un explorador vacio.
        /// </summary>
        public ExploradorViewModel()
        {
            this.Nodos = new ObservableCollection<NodoViewModel>();
            this.NodoTablaActual = new NodoViewModel();
            this.RutaNodoActual = string.Empty;
            this.conexion = null;
        }

        /// <summary>
        /// Construye un explorador con determinado arbol de nodos y proveedor de datos.
        /// </summary>
        /// <param name="Nodos">Árbol de nodos a emplear. Si se trata de un explorador de carga
        /// de nodos por demanda (lazy loading), el árbol pasado como parámetro por lo general 
        /// sólo contiene el nodo inicial.</param>
        /// <param name="Conexion">Proveedor de datos. A través de este se obtienen los datos y metadatos
        /// de los nodos del árbol.</param>
        public ExploradorViewModel(ObservableCollection<NodoViewModel> Nodos, ConexionViewModel Conexion)
        {
            this.Nodos = Nodos;
            AsignarEsteExploradorA(Nodos);

            this.NodoActual = Nodos[0];
            this.NodoTablaActual = new NodoViewModel();
            this.RutaNodoActual = string.Empty;
            this.conexion = Conexion;
        }

        ~ExploradorViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Representa la colección de nodos que conforman el arbol de datos.
        /// </summary>
        public ObservableCollection<NodoViewModel> Nodos { get; private set; }

        /// <summary>
        /// Lee o escribe la cache de tablas en la entrada especificada por <see cref="NodoTablaActual"/>.
        /// </summary>
        public DataTable TablaActual
        {
            get { return NodoTablaActual.ExisteEnRepositorioDeTablas() ? NodoTablaActual.BuscarEnRepositorioDeTablas() : null; }
            set
            {
                if (NodoTablaActual.ExisteEnRepositorioDeTablas())
                {
                    if (NodoTablaActual.BuscarEnRepositorioDeTablas() != value)
                    {
                        NodoTablaActual.AgregarARepositorioDeTablas(value);
                    }
                }
                else
                {
                    NodoTablaActual.AgregarARepositorioDeTablas(value);
                }

                this.RaisePropertyChanged("TablaActual");
            }
        }

        /// <summary>
        /// Indica el nodo actual seleccionado.
        /// </summary>
        public NodoViewModel NodoActual
        {
            get { return nodoActual; }
            set
            {
                if (value != nodoActual)
                {
                    nodoActual = value;
                    RutaNodoActual = nodoActual.RutaCompleta();
                    this.RaisePropertyChanged("RutaNodoActual");
                    this.RaisePropertyChanged("NodoActual");
                }
            }
        }

        /// <summary>
        /// Este string indica la ruta completa del nodo actual seleccionado.
        /// </summary>
        public string RutaNodoActual { get; private set; }

        /// <summary>
        /// Indica el nodo asociado a la tabla actual (no necesariamente es igual a <see cref="NodoActual"/>).
        /// </summary>
        public NodoViewModel NodoTablaActual { get; set; }
        
        /// <summary>
        /// Expande el nodo especificado
        /// </summary>
        public ICommand ExpandirOrden
        {
            get { return expandirOrden ?? (expandirOrden 
                = new RelayCommand<NodoViewModel>(Nodo => this.ExpandirAccion(Nodo))); }
        }
        
        /// <summary>
        /// Establece NodoActual.
        /// </summary>
        public ICommand EstablecerNodoActualOrden
        {
            get { return establecerNodoActualOrden ?? (establecerNodoActualOrden = 
                new RelayCommand<string>(Nombre => this.EstablecerNodoActualAccion(Nombre))); }
        }

        /// <summary>
        /// Establece el modo de las operaciones de lectura/escritura (sincróncias o asincrónicas)
        /// sobre la fuente de datos subyacente.
        /// </summary>
        public bool OperacionAsincronica { get; set; }

        #endregion
    }
}
