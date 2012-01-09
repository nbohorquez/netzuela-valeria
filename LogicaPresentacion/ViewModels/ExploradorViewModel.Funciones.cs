using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // DataTable
using System.Windows;                           // MessageBox
using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes, ExpresionGenerica
using Zuliaworks.Netzuela.Valeria.Datos;        // EventoEnviarTablasCompletadoArgs

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public partial class ExploradorViewModel
    {
        #region Funciones

        #region Otras

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

        #endregion

        #region Acciones

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
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        #endregion

        #region Expandir

        private void ExpandirServidor(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            if (Item.Expandido == true)
                return;

            EventHandler<EventoOperacionAsincCompletadaArgs> Retorno = null;
            string[] BasesDeDatos = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
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
            };

            Retorno = (r, a) =>
            {
                try
                {
                    _Conexion.ListarBasesDeDatosCompletado -= Retorno;
                    BasesDeDatos = a.Resultado as string[];
                    CrearNodos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            try
            {
                // Aqui seria mejor revisar una variable de configuracion del usuario
                // que indique si se deben realizan llamadas a los procedimientos 
                // remotos/locales de forma asincronica o sincronica
                if (_Conexion.Parametros.Servidor == Constantes.SGBDR.NETZUELA)
                {
                    _Conexion.ListarBasesDeDatosCompletado += Retorno;
                    _Conexion.ListarBasesDeDatosAsinc();
                }
                else
                {
                    BasesDeDatos = _Conexion.ListarBasesDeDatos();
                    CrearNodos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ExpandirBaseDeDatos(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            if (Item.Expandido == true)
                return;

            EventHandler<EventoOperacionAsincCompletadaArgs> Retorno = null;
            string[] Tablas = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
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
            };

            Retorno = (r, a) =>
            {
                try
                {
                    _Conexion.ListarTablasCompletado -= Retorno;
                    Tablas = a.Resultado as string[];
                    CrearNodos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            try
            {
                // Aqui seria mejor revisar una variable de configuracion del usuario
                // que indique si se deben realizan llamadas a los procedimientos 
                // remotos/locales de forma asincronica o sincronica
                if (_Conexion.Parametros.Servidor == Constantes.SGBDR.NETZUELA)
                {
                    _Conexion.ListarTablasCompletado += Retorno;
                    _Conexion.ListarTablasAsinc(Item.Nombre);
                }
                else
                {
                    Tablas = _Conexion.ListarTablas(Item.Nombre);
                    CrearNodos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void ExpandirTabla(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            DataTable Tabla = null;
            EventHandler<EventoOperacionAsincCompletadaArgs> Retorno = null;

            ExpresionGenerica AjustarExplorador = () =>
            {
                Item.Expandido = true;

                /* 
                    * NodoTablaActual esta atado a TablaActual: el primero es el indice dentro del 
                    * diccionario Tablas para ubicar el segundo.
                    */

                NodoTablaActual = Item;
                TablaActual = Tabla;
            };

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (Tabla != null)
                {
                    Tabla.TableName = Item.RutaCompleta();

                    foreach (NodoViewModel N in Item.Hijos)
                    {
                        N.Dispose();
                    }

                    Item.Hijos.Clear();

                    foreach (DataColumn Columna in Tabla.Columns)
                    {
                        NodoViewModel N = new NodoViewModel(Columna.ColumnName, Item);
                    }

                    AjustarExplorador();
                }
            };

            Retorno = (r, a) =>
            {
                try
                {
                    _Conexion.LeerTablaCompletado -= Retorno;
                    Tabla = a.Resultado as DataTable;
                    CrearNodos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            try
            {
                if (_CacheDeTablas.ContainsKey(Item))
                {
                    Tabla = _CacheDeTablas[Item];
                    AjustarExplorador();
                }
                else
                {
                    // Aqui seria mejor revisar una variable de configuracion del usuario
                    // que indique si se deben realizan llamadas a los procedimientos 
                    // remotos/locales de forma asincronica o sincronica
                    if (_Conexion.Parametros.Servidor == Constantes.SGBDR.NETZUELA)
                    {
                        _Conexion.LeerTablaCompletado += Retorno;
                        _Conexion.LeerTablaAsinc(Item.Padre.Nombre, Item.Nombre);
                    }
                    else
                    {
                        Tabla = _Conexion.LeerTabla(Item.Padre.Nombre, Item.Nombre);
                        CrearNodos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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

        #endregion

        #region Funciones de tablas

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

            if (Tabla.Nivel == Constantes.NivelDeNodo.TABLA)
            {
                if (_CacheDeTablas.ContainsKey(Tabla))
                {
                    Resultado = _CacheDeTablas[Tabla];
                }
            }

            return Resultado;
        }

        public void EscribirTabla(NodoViewModel Nodo, DataTable Tabla)
        {
            if (Nodo.Nivel != Constantes.NivelDeNodo.TABLA)
                return;

            try
            {
                _Conexion.EscribirTabla(Nodo.Padre.Nombre, Nodo.Nombre, Tabla);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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

        #endregion
    }
}
