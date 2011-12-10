using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // DataTable
using System.Windows;                           // MessageBox
using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public partial class ExploradorViewModel
    {
        #region Funciones

        private void AsignarEsteExploradorA(ObservableCollection<NodoViewModel> Nodos)
        {
            foreach (NodoViewModel Nodo in Nodos)
            {
                Nodo.Explorador = this;
                if (Nodo.Hijos.Count > 0)
                {
                    AsignarEsteExploradorA(Nodo.Hijos);
                }
            }
        }

        private void EstablecerNodoActualAccion(string Nombre)
        {
            this.NodoActual = (Nombre == null)
                ? this.NodoActual
                : NodoViewModelExtensiones.BuscarNodo(Nombre, NodoTablaActual.Hijos);
        }

        private void ExpandirAccion(NodoViewModel Nodo)
        {
            try
            {
                Expandir(Nodo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        private void ExpandirServidor(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            if (Item.Expandido == true)
                return;

            try
            {
                string[] BasesDeDatos = _BD.ListarBasesDeDatos();

                if (BasesDeDatos != null)
                {
                    Item.Expandido = true;

                    foreach (NodoViewModel N in Item.Hijos)
                    {
                        N.Dispose();
                    }
                    
                    Item.Hijos.Clear();

                    foreach (string BdD in BasesDeDatos)
                    {
                        NodoViewModel Nodo = new NodoViewModel(BdD, Item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        private void ExpandirBaseDeDatos(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            if (Item.Expandido == true)
                return;

            try
            {
                string[] Tablas = _BD.ListarTablas(Item.Nombre);

                if (Tablas != null)
                {
                    Item.Expandido = true;

                    foreach (NodoViewModel N in Item.Hijos)
                    {
                        N.Dispose();
                    }

                    Item.Hijos.Clear();

                    foreach (string Tabla in Tablas)
                    {
                        NodoViewModel Nodo = new NodoViewModel(Tabla, Item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
        }

        private void ExpandirTabla(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            DataTable Tabla = ObtenerTabla(Item);

            if (Tabla != null)
            {
                Item.Expandido = true;

                /* 
                 * NodoTablaActual esta atado a TablaActual: el primero es el indice dentro del 
                 * diccionario Tablas para ubicar el segundo.
                 */

                NodoTablaActual = Item;
                TablaActual = Tabla;
            }
        }

        private void ExpandirColumna(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            NodoViewModel Padre = Item.Padre;
            ExpandirTabla(Padre);

            Item.Expandido = true;
        }

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

            try
            {
                if (Tabla.Nivel == Constantes.NivelDeNodo.TABLA)
                {
                    if (_CacheDeTablas.ContainsKey(Tabla))
                    {
                        Temp = _CacheDeTablas[Tabla];
                    }
                    else
                    {
                        Temp = _BD.LeerTabla(Tabla.Padre.Nombre, Tabla.Nombre);
                        Temp.TableName = Tabla.RutaCompleta();

                        foreach (NodoViewModel N in Tabla.Hijos)
                        {
                            N.Dispose();
                        }
                        Tabla.Hijos.Clear();

                        foreach (DataColumn Columna in Temp.Columns)
                        {
                            NodoViewModel N = new NodoViewModel(Columna.ColumnName, Tabla);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.InnerException.Message);
            }

            return Temp;
        }

        public void EscribirTabla(NodoViewModel Nodo, DataTable Tabla)
        {
            if (Nodo.Nivel != Constantes.NivelDeNodo.TABLA)
                return;

            try
            {
                _BD.EscribirTabla(Nodo.Padre.Nombre, Nodo.Nombre, Tabla);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException.Message);
            }
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

        #endregion
    }
}
