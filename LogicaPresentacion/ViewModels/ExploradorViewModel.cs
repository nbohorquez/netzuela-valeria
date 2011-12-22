using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // DataTable
using System.Windows;                           // MessageBox
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Esta clase se emplea para acceder a los datos de una fuente cuya estructura 
    /// sea un árbol de nodos.
    /// </summary>
    public partial class ExploradorViewModel : ObservableObject
    {
        #region Variables

        private Dictionary<NodoViewModel, DataTable> _CacheDeTablas;
        private IBaseDeDatos _BD;
        private NodoViewModel _NodoActual;
        private RelayCommand<NodoViewModel> _ExpandirOrden;
        private RelayCommand<string> _EstablecerNodoActualOrden;

        #endregion

        #region Constructores

        /// <summary>
        /// Construye un explorador vacio.
        /// </summary>
        public ExploradorViewModel()
        {
            this.Nodos = new ObservableCollection<NodoViewModel>();
            this._CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
            this.NodoTablaActual = new NodoViewModel();
            this.RutaNodoActual = string.Empty;
            this._BD = null;
        }

        /// <summary>
        /// Construye un explorador con determinado arbol de nodos y proveedor de datos.
        /// </summary>
        /// <param name="Nodos">Árbol de nodos a emplear. Si se trata de un explorador de carga
        /// de nodos por demanda (lazy loading), el árbol pasado como parámetro por lo general 
        /// sólo contiene el nodo inicial.</param>
        /// <param name="BD">Proveedor de datos. A través de este se obtienen los datos y metadatos
        /// de los nodos del árbol.</param>
        public ExploradorViewModel(ObservableCollection<NodoViewModel> Nodos, IBaseDeDatos BD)
        {
            this.Nodos = Nodos;
            AsignarEsteExploradorA(Nodos);

            this._CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
            this.NodoActual = Nodos[0];
            this.NodoTablaActual = new NodoViewModel();
            this.RutaNodoActual = string.Empty;
            this._BD = BD;
            
            // ¡¡¡HEY ESTA VERGA HAY QUE CAMBIARLA!!!
            if (_BD.DatosDeConexion.Servidor == Comunes.Constantes.SGBDR.NETZUELA)
            {
                this._BD.EnviarTablasCompletado = EscribirTablaRetorno;
            }
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
            get { return _CacheDeTablas.ContainsKey(NodoTablaActual) ? _CacheDeTablas[NodoTablaActual] : null; }
            set
            {
                if (_CacheDeTablas.ContainsKey(NodoTablaActual))
                {
                    if (_CacheDeTablas[NodoTablaActual] != value)
                        _CacheDeTablas[NodoTablaActual] = value;                        
                }
                else
                {
                    _CacheDeTablas.Add(NodoTablaActual, value);
                }

                RaisePropertyChanged("TablaActual");
            }
        }

        /// <summary>
        /// Indica el nodo actual seleccionado.
        /// </summary>
        public NodoViewModel NodoActual
        {
            get { return _NodoActual; }
            set
            {
                if (value != _NodoActual)
                {
                    _NodoActual = value;
                    RutaNodoActual = _NodoActual.RutaCompleta();
                    RaisePropertyChanged("RutaNodoActual");
                    RaisePropertyChanged("NodoActual");
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
            get { return _ExpandirOrden ?? (_ExpandirOrden 
                = new RelayCommand<NodoViewModel>(Nodo => this.ExpandirAccion(Nodo))); }
        }
        
        /// <summary>
        /// Establece NodoActual
        /// </summary>
        public ICommand EstablecerNodoActualOrden
        {
            get { return _EstablecerNodoActualOrden ?? (_EstablecerNodoActualOrden = 
                new RelayCommand<string>(Nombre => this.EstablecerNodoActualAccion(Nombre))); }
        }

        #endregion
    }
}
