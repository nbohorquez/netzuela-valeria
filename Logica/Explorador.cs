using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                          // DataTable
using System.Collections.ObjectModel;       // ObservableCollection
using System.ComponentModel;                // INotifyPropertyChanged
using Zuliaworks.Netzuela.Valeria.Datos;                        // BaseDeDatos
using Zuliaworks.Netzuela.Valeria.Comunes;                      // Constantes

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Esta clase se emplea para acceder a los datos de una fuente cuya estructura 
    /// sea de tipo árbol de nodos. Implementa la interfaz <see cref="INotifyPropertyChanged"/>
    /// para hacer visibles algunas de sus propiedades a la capa de presentación.
    /// </summary>
    public class Explorador : INotifyPropertyChanged
    {
        #region Variables

        /// <summary>
        /// Indica el nodo asociado a la tabla actual (no necesariamente es igual a <see cref="NodoActual"/>).
        /// </summary>
        public Nodo NodoTablaActual;

        /// <summary>
        /// Es la caché de tablas del explorador.
        /// </summary>
        private Dictionary<Nodo, DataTable> Tablas;

        /// <summary>
        /// Es el proveedor de datos del explorador.
        /// </summary>
        private IBaseDeDatos BD;

        /// <summary>
        /// Indica el nodo actual seleccionado.
        /// </summary>
        private Nodo _NodoActual;

        #endregion

        #region Constructores

        /// <summary>
        /// Construye un explorador vacio.
        /// </summary>
        public Explorador()
        {
            this.Nodos = new ObservableCollection<Nodo>();
            this.Tablas = new Dictionary<Nodo, DataTable>();
            this.NodoActual = new Nodo();
            this.NodoTablaActual = new Nodo();
            this.BD = null;
        }

        /// <summary>
        /// Construye un explorador con determinado arbol de nodos y proveedor de datos.
        /// </summary>
        /// <param name="Nodos">Árbol de nodos a emplear. Si se trata de un explorador de carga
        /// de nodos por demanda (lazy loading), el árbol pasado como parámetro por lo general 
        /// sólo contiene el nodo inicial.</param>
        /// <param name="BD">Proveedor de datos. A través de este se obtiene la data y metadata 
        /// de los nodos del árbol.</param>
        public Explorador(ObservableCollection<Nodo> Nodos, IBaseDeDatos BD)
        {
            this.Nodos = Nodos;
            AsignarExplorador(this.Nodos, this);

            this.Tablas = new Dictionary<Nodo, DataTable>();
            this.NodoActual = Nodos[0];
            this.NodoTablaActual = new Nodo();
            this.BD = BD;
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Lee o escribe la cache de tablas en la entrada especificada por <see cref="NodoTablaActual"/>.
        /// </summary>
        public DataTable TablaActual
        {
            get { return Tablas.ContainsKey(NodoTablaActual) ? Tablas[NodoTablaActual] : null; }
            set
            {
                if (Tablas.ContainsKey(NodoTablaActual))
                {
                    if (Tablas[NodoTablaActual] != value)
                        Tablas[NodoTablaActual] = value;
                }
                else
                    Tablas.Add(NodoTablaActual, value);

                RegistrarCambioEnPropiedad("TablaActual");
            }
        }

        /// <summary>
        /// Indica el nodo actual seleccionado.
        /// </summary>
        public Nodo NodoActual
        {
            get { return _NodoActual; }
            set
            {
                if (value != _NodoActual)
                {
                    _NodoActual = value;
                    RegistrarCambioEnPropiedad("NodoActual");
                }
            }
        }

        /// <summary>
        /// Representa la colección de nodos que conforman el arbol de datos.
        /// </summary>
        public ObservableCollection<Nodo> Nodos { get; set; }

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
        public void ExpandirTodo(ObservableCollection<Nodo> Nodos)
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
        public void Expandir(Nodo Item)
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

        private void ExpandirServidor(Nodo Item)
        {
            if (Item.Expandido == true)
                return;

            Item.Expandido = true;

            string[] BasesDeDatos = BD.ListarBasesDeDatos();

            if (BasesDeDatos != null)
            {
                Item.Hijos.Clear();
                foreach (string BdD in BasesDeDatos)
                {
                    Nodo Nodo = new Nodo(BdD);
                    Item.AgregarHijo(Nodo);
                }                
            }                        
        }

        private void ExpandirBaseDeDatos(Nodo Item)
        {
            if (Item.Expandido == true)
                return;

            Item.Expandido = true;

            string[] Tablas = BD.ListarTablas(Item.Nombre);

            if (Tablas != null)
            {
                Item.Hijos.Clear();
                foreach (string Tabla in Tablas)
                {
                    Nodo Nodo = new Nodo(Tabla);
                    Item.AgregarHijo(Nodo);
                }
            }
        }

        private void ExpandirTabla(Nodo Item)
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

        private void ExpandirColumna(Nodo Item)
        {
            Item.Expandido = true;

            Nodo Padre = Item.Padre;
            ExpandirTabla(Padre);
        }

        /// <summary>
        /// Lee la tabla especificada desde el proveedor de datos.
        /// </summary>
        /// <param name="Tabla">Nodo del árbol de datos cuya tabla se quiere obtener</param>
        /// <returns>Tabla leída desde el proveedor de datos o nulo si no se pudo encontrar.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="Tabla"/> es una referencia 
        /// nula.</exception>
        public DataTable ObtenerTabla(Nodo Tabla)
        {
            if (Tabla == null)
                throw new ArgumentNullException("Tabla");

            DataTable Temp = null;

            if (Tabla.Nivel == Constantes.NivelDeNodo.TABLA)
            {
                if (Tablas.ContainsKey(Tabla))
                {
                    Temp = Tablas[Tabla];
                }
                else
                {
                    Temp = BD.MostrarTabla(Tabla.Padre.Nombre, Tabla.Nombre);
                    Temp.TableName = Nodo.RutaCompleta(Tabla);

                    Tabla.Hijos.Clear();
                    foreach (DataColumn Columna in Temp.Columns)
                    {
                        Nodo N = new Nodo(Columna.ColumnName);
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
            return Tablas.Values.ToList();
        }

        /// <summary>
        /// Devuelve los nodos asociados a las tablas de la caché de tablas.
        /// </summary>
        /// <returns>Lista de los nodos de tablas guardados en la caché.</returns>
        public List<Nodo> ObtenerNodosCache()
        {
            return Tablas.Keys.ToList();
        }

        private void AsignarExplorador(ObservableCollection<Nodo> Nodos, Explorador Arbol)
        {
            foreach (Nodo Nodo in Nodos)
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

        /// <summary>
        /// Evento que se activa cuando una propiedad de esta clase ha sido modificada.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Esta función se llama de forma interna cuando se cambia una propiedad de esta clase
        /// </summary>
        /// <param name="info">Nombre de la propiedad modificada.</param>
        protected virtual void RegistrarCambioEnPropiedad(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
