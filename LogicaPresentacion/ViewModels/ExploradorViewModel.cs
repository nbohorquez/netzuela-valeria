using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // DataTable
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Esta clase se emplea para acceder a los datos de una fuente cuya estructura 
    /// sea un árbol de nodos.
    /// </summary>
    public class ExploradorViewModel : ObservableObject
    {
        #region Variables

        /// <summary>
        /// Es la caché de tablas del explorador.
        /// </summary>
        private Dictionary<NodoViewModel, DataTable> _CacheDeTablas;

        /// <summary>
        /// Es el proveedor de datos del explorador.
        /// </summary>
        private IBaseDeDatos _BD;

        /// <summary>
        /// Indica el nodo actual seleccionado.
        /// </summary>
        private NodoViewModel _NodoActual;

        /// <summary>
        /// Esta orden 
        /// </summary>
        private RelayCommand _ExpandirNodo;    

        #endregion

        #region Constructores

        /// <summary>
        /// Construye un explorador vacio.
        /// </summary>
        public ExploradorViewModel()
        {
            this.Nodos = new ObservableCollection<NodoViewModel>();
            this._CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
            this.NodoActual = new NodoViewModel();
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
        /// <param name="BD">Proveedor de datos. A través de este se obtiene la data y metadata 
        /// de los nodos del árbol.</param>
        public ExploradorViewModel(ObservableCollection<NodoViewModel> Nodos, IBaseDeDatos BD)
        {
            this.Nodos = Nodos;
            AsignarExplorador(this.Nodos, this);

            this._CacheDeTablas = new Dictionary<NodoViewModel, DataTable>();
            this.NodoActual = Nodos[0];
            this.NodoTablaActual = new NodoViewModel();
            this.RutaNodoActual = string.Empty;
            this._BD = BD;
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Representa la colección de nodos que conforman el arbol de datos.
        /// </summary>
        public ObservableCollection<NodoViewModel> Nodos { get; private set; }

        /// <summary>
        /// Lee o escribe la cache de tablas en la entrada especificada por <see cref="_NodoTablaActual"/>.
        /// </summary>
        public DataTable TablaActual
        {
            get { return _CacheDeTablas.ContainsKey(NodoTablaActual) ? _CacheDeTablas[NodoTablaActual] : null; }
            private set
            {
                if (_CacheDeTablas.ContainsKey(NodoTablaActual))
                {
                    if (_CacheDeTablas[NodoTablaActual] != value)
                        _CacheDeTablas[NodoTablaActual] = value;
                }
                else
                    _CacheDeTablas.Add(NodoTablaActual, value);

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
        public NodoViewModel NodoTablaActual { get; private set; }

        public ICommand ExpandirNodo
        {
            get { return _ExpandirNodo ?? (_ExpandirNodo = new RelayCommand(this.)); }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        /// <summary>
        /// Expande todos los nodos del árbol de nodos <see cref="Nodos"/> de este explorador.
        /// </summary>
        public void ExpandirTodo()
        {
            ExpandirTodo(this.Nodos);
        }

        /// <summary>
        /// Expande todos los nodos del árbol de nodos especificado.
        /// </summary>
        /// <param name="Nodos">Arbol a expandir.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="Nodos"/> es una referencia 
        /// nula.</exception>
        public void ExpandirTodo(ObservableCollection<NodoViewModel> Nodos)
        {
            if (Nodos == null)
                throw new ArgumentNullException("Nodos");

            // Este mismo ciclo no quiso funcionar con un foreach porque se perdia la numeracion
            for (int i = 0; i < Nodos.Count; i++)
            {
                Expandir(Nodos[i]);

                if (Nodos[i].Hijos.Count > 0)
                    ExpandirTodo(Nodos[i].Hijos);
            }
        }

        /// <summary>
        /// Obtiene el contenido del nodo especificado desde el proveedor de datos.
        /// </summary>
        /// <param name="Item">Nodo a expandir. El nivel de este nodo solo puede ser uno de los 
        /// especificados en <see cref="Constantes.NivelDeNodo"/>.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="Item"/> es una referencia 
        /// nula.</exception>
        public void Expandir(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            switch (Item.Nivel)
            {
                case Constantes.NivelDeNodo.SERVIDOR:
                    ExpandirServidor(Item);
                    break;
                case Constantes.NivelDeNodo.BASE_DE_DATOS:
                    ExpandirBaseDeDatos(Item);
                    break;
                case Constantes.NivelDeNodo.TABLA:
                    ExpandirTabla(Item);
                    break;
                case Constantes.NivelDeNodo.COLUMNA:
                    ExpandirColumna(Item);
                    break;
                default:
                    break;
            }

            NodoActual = Item;
        }

        private void ExpandirServidor(NodoViewModel Item)
        {
            if (Item.Expandido == true)
                return;

            Item.Expandido = true;

            string[] BasesDeDatos = _BD.ListarBasesDeDatos();

            if (BasesDeDatos != null)
            {
                Item.Hijos.Clear();
                foreach (string BdD in BasesDeDatos)
                {
                    NodoViewModel Nodo = new NodoViewModel(BdD);
                    Item.AgregarHijo(Nodo);
                }
            }
        }

        private void ExpandirBaseDeDatos(NodoViewModel Item)
        {
            if (Item.Expandido == true)
                return;

            Item.Expandido = true;

            string[] Tablas = _BD.ListarTablas(Item.Nombre);

            if (Tablas != null)
            {
                Item.Hijos.Clear();
                foreach (string Tabla in Tablas)
                {
                    NodoViewModel Nodo = new NodoViewModel(Tabla);
                    Item.AgregarHijo(Nodo);
                }
            }
        }

        private void ExpandirTabla(NodoViewModel Item)
        {
            Item.Expandido = true;

            DataTable Tabla = ObtenerTabla(Item);

            if (Tabla != null)
            {
                /* 
                 * nTablaActual esta atado a TablaActual: el primero es el indice dentro del 
                 * diccionario Tablas para ubicar el segundo.
                 */

                NodoTablaActual = Item;
                TablaActual = Tabla;
            }
        }

        private void ExpandirColumna(NodoViewModel Item)
        {
            Item.Expandido = true;

            NodoViewModel Padre = Item.Padre;
            ExpandirTabla(Padre);
        }

        /// <summary>
        /// Lee la tabla especificada desde el proveedor de datos.
        /// </summary>
        /// <param name="Tabla">Nodo del árbol de datos cuya tabla se quiere obtener</param>
        /// <returns>Tabla leída desde el proveedor de datos o nulo si no se pudo encontrar.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="Tabla"/> es una referencia 
        /// nula.</exception>
        public DataTable ObtenerTabla(NodoViewModel Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            DataTable Temp = null;

            if (Tabla.Nivel == Constantes.NivelDeNodo.TABLA)
            {
                if (_CacheDeTablas.ContainsKey(Tabla))
                {
                    Temp = _CacheDeTablas[Tabla];
                }
                else
                {
                    Temp = _BD.MostrarTabla(Tabla.Padre.Nombre, Tabla.Nombre);
                    Temp.TableName = Tabla.RutaCompleta();

                    Tabla.Hijos.Clear();
                    foreach (DataColumn Columna in Temp.Columns)
                    {
                        NodoViewModel N = new NodoViewModel(Columna.ColumnName);
                        N.Hijos.Clear();
                        Tabla.AgregarHijo(N);
                    }
                }
            }

            return Temp;
        }

        /// <summary>
        /// Devuelve las tablas presentes en la caché de tablas.
        /// </summary>
        /// <returns>Lista de las tablas guardadas en la caché.</returns>
        public List<DataTable> ObtenerTablasCache()
        {
            return _CacheDeTablas.Values.ToList();
        }

        /// <summary>
        /// Devuelve los nodos asociados a las tablas de la caché de tablas.
        /// </summary>
        /// <returns>Lista de los nodos de tablas guardados en la caché.</returns>
        public List<NodoViewModel> ObtenerNodosCache()
        {
            return _CacheDeTablas.Keys.ToList();
        }

        private void AsignarExplorador(ObservableCollection<NodoViewModel> Nodos, ExploradorViewModel Arbol)
        {
            NodoViewModel Nodito = new NodoViewModel();

            foreach (NodoViewModel Nodo in Nodos)
            {
                Nodo.Explorador = this;
                if (Nodo.Hijos.Count > 0)
                {
                    AsignarExplorador(Nodo.Hijos, this);
                }
            }
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
