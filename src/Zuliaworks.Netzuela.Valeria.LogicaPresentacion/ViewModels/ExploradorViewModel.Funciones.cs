namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Data;                              // DataTable
    using System.Linq;
    using System.Text;
    using System.Windows;                           // MessageBox
    
    using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes, ExpresionGenerica
    using Zuliaworks.Netzuela.Valeria.Datos;        // EventoEnviarTablasCompletadoArgs
    
    public partial class ExploradorViewModel : IDisposable
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

        protected void EstablecerNodoActualAccion(string Nombre)
        {
            try
            {
                this.NodoActual = (Nombre == null)
                    ? this.NodoActual
                    : NodoViewModelExtensiones.BuscarNodo(Nombre, NodoTablaActual.Hijos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected void ExpandirAccion(NodoViewModel Nodo)
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

            EventHandler<EventoListarBDsCompletadoArgs> Retorno = null;
            string[] BasesDeDatos = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (BasesDeDatos != null)
                {
                    Item.Expandido = true;

                    var Eliminar = from Hijo in Item.Hijos
                                   where !(from BD in BasesDeDatos
                                           select BD).Contains(Hijo.Nombre)
                                   select Hijo;

                    var Agregar = from BD in BasesDeDatos
                                  where !(from Hijo in Item.Hijos
                                          select Hijo.Nombre).Contains(BD)
                                  select BD;

                    foreach (var Hijo in Eliminar)
                    {
                        Hijo.Dispose();
                    }

                    foreach (var BD in Agregar)
                    {
                        NodoViewModel N = new NodoViewModel(BD, Item);
                    }
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            Retorno = (r, a) =>
            {
                // Esto va metido entre try/cath porque se ejecuta solo
                try
                {
                    _Conexion.ListarBasesDeDatosCompletado -= Retorno;

                    if (a.Error != null)
                    {
                        throw a.Error;
                    }
                    else if (a.Cancelled)
                    {
                        MessageBox.Show("Operacion LeerTablaAsinc cancelada");
                    }
                    else if (a.Resultado != null)
                    {
                        BasesDeDatos = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (OperacionAsincronica)
            {
                this._Conexion.ListarBasesDeDatosCompletado -= Retorno;
                this._Conexion.ListarBasesDeDatosCompletado += Retorno;
                this._Conexion.ListarBasesDeDatosAsinc();
            }
            else
            {
                BasesDeDatos = this._Conexion.ListarBasesDeDatos();
                CrearNodos();
            }
        }

        private void ExpandirBaseDeDatos(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            if (Item.Expandido == true)
                return;

            EventHandler<EventoListarTablasCompletadoArgs> Retorno = null;
            string[] Tablas = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (Tablas != null)
                {
                    Item.Expandido = true;

                    var Eliminar = from Hijo in Item.Hijos
                                   where !(from Tabla in Tablas
                                           select Tabla).Contains(Hijo.Nombre)
                                   select Hijo;

                    var Agregar = from Tabla in Tablas
                                  where !(from Hijo in Item.Hijos
                                          select Hijo.Nombre).Contains(Tabla)
                                  select Tabla;

                    foreach (var Hijo in Eliminar)
                    {
                        Hijo.Dispose();
                    }

                    foreach (var Tabla in Agregar)
                    {
                        NodoViewModel N = new NodoViewModel(Tabla, Item);
                    }
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            Retorno = (r, a) =>
            {
                try
                {
                    this._Conexion.ListarTablasCompletado -= Retorno;

                    if (a.Error != null)
                    {
                        throw a.Error;
                    }
                    else if (a.Cancelled)
                    {
                        MessageBox.Show("Operacion LeerTablaAsinc cancelada");
                    }
                    else if (a.Resultado != null)
                    {
                        Tablas = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (OperacionAsincronica)
            {
                this._Conexion.ListarTablasCompletado -= Retorno;
                this._Conexion.ListarTablasCompletado += Retorno;
                this._Conexion.ListarTablasAsinc(Item.Nombre);
            }
            else
            {
                Tablas = this._Conexion.ListarTablas(Item.Nombre);
                CrearNodos();
            }
        }

        private void ExpandirTabla(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            DataTable Tabla = null;
            EventHandler<EventoLeerTablaCompletadoArgs> Retorno = null;

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
                    //Tabla.TableName = Item.RutaCompleta();
                    Tabla.TableName = Item.Nombre;

                    var Eliminar = from Hijo in Item.Hijos
                                   where !(from Columna in Tabla.Columns.Cast<DataColumn>()
                                           select Columna.ColumnName).Contains(Hijo.Nombre)
                                   select Hijo;

                    var Agregar = from Columna in Tabla.Columns.Cast<DataColumn>()
                                  where !(from Hijo in Item.Hijos
                                          select Hijo.Nombre).Contains(Columna.ColumnName)
                                  select Columna;

                    foreach (var Hijo in Eliminar)
                    {
                        Hijo.Dispose();
                    }

                    foreach (var Columna in Agregar)
                    {
                        NodoViewModel N = new NodoViewModel(Columna.ColumnName, Item);
                    }
                                        
                    AjustarExplorador();
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            Retorno = (r, a) =>
            {
                try
                {
                    this._Conexion.LeerTablaCompletado -= Retorno;

                    if (a.Error != null)
                    {
                        throw a.Error;
                    }
                    else if (a.Cancelled)
                    {
                        MessageBox.Show("Operacion LeerTablaAsinc cancelada");
                    }
                    else if (a.Resultado != null)
                    {
                        Tabla = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (Item.ExisteEnRepositorioDeTablas())
            {
                Tabla = Item.BuscarEnRepositorioDeTablas();
                AjustarExplorador();
            }
            else
            {
                if (OperacionAsincronica)
                {
                    this._Conexion.LeerTablaCompletado -= Retorno;
                    this._Conexion.LeerTablaCompletado += Retorno;
                    this._Conexion.LeerTablaAsinc(Item.Padre.Nombre, Item.Nombre);
                }
                else
                {
                    Tabla = this._Conexion.LeerTabla(Item.Padre.Nombre, Item.Nombre);
                    CrearNodos();
                }
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

        public void ExpandirRuta(string Ruta)
        {
            string[] PasosDeLaRuta = Ruta.Split('\\');

            ObservableCollection<NodoViewModel> Lista = Nodos;
            NodoViewModel Nodo = null;

            try
            {
                foreach (string Paso in PasosDeLaRuta)
                {
                    if (Paso == string.Empty)
                        continue;

                    Nodo = NodoViewModelExtensiones.RutaANodo(Paso, Lista);

                    if (Nodo != null)
                    {
                        Expandir(Nodo);
                        Lista = Nodo.Hijos;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al expandir la ruta: " + Ruta, ex);
            }
        }

        /// <summary>
        /// Expande todos los nodos del árbol de nodos <see cref="Nodos"/> de este explorador.
        /// </summary>
        public void ExpandirTodo()
        {
            try
            {
                ExpandirTodo(this.Nodos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

            try
            {
                // Este mismo ciclo no quiso funcionar con un foreach porque se perdia la numeracion
                for (int i = 0; i < Nodos.Count; i++)
                {
                    Expandir(Nodos[i]);

                    if (Nodos[i].Hijos.Count > 0)
                        ExpandirTodo(Nodos[i].Hijos);
                }
            }
            catch (Exception ex)
            {
                throw ex;
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

            try
            {
                switch (Item.Nivel)
                {
                    case NivelDeNodo.Servidor:
                        ExpandirServidor(Item);
                        break;
                    case NivelDeNodo.BaseDeDatos:
                        ExpandirBaseDeDatos(Item);
                        break;
                    case NivelDeNodo.Tabla:
                        ExpandirTabla(Item);
                        break;
                    case NivelDeNodo.Columna:
                        ExpandirColumna(Item);
                        break;
                    default:
                        break;
                }

                NodoActual = Item;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al expandir el NodoViewModel " + Item.Nombre, ex); 
            }
        }

        public void Reexpandir(NodoViewModel Item)
        {
            if (Item == null)
                throw new ArgumentNullException("Item");

            Item.Expandido = false;

            if (Item.ExisteEnRepositorioDeTablas())
            {
                Item.QuitarDeRepositorioDeTablas();
            }

            Expandir(Item);
        }

        #endregion

        #region Funciones de tablas

        public bool EscribirTabla(NodoViewModel Nodo, DataTable Tabla)
        {
            if (Nodo.Nivel != NivelDeNodo.Tabla)
                return false;

            bool Resultado = false;
            EventHandler<EventoEscribirTablaCompletadoArgs> Retorno = null;

            Retorno = (r, a) =>
            {
                // Esto va metido entre try/cath porque se ejecuta solo
                try
                {
                    bool R = false;
                    string Mensaje = string.Empty;

                    this._Conexion.EscribirTablaCompletado -= Retorno;

                    if (a.Error != null)
                    {
                        throw a.Error;
                    }
                    else if (a.Cancelled)
                    {
                        Mensaje += "Operacion EscribirTablaAsinc cancelada";
                    }
                    else if (a.Resultado != null)
                    {
                        R = a.Resultado;
                        Mensaje += "El resultado de la operacion EscribirTablaAsinc fue: " + R.ToString();
                    }

                    MessageBox.Show(Mensaje);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            try
            {
                if (OperacionAsincronica)
                {
                    this._Conexion.EscribirTablaCompletado -= Retorno;
                    this._Conexion.EscribirTablaCompletado += Retorno;
                    this._Conexion.EscribirTablaAsinc(Nodo.Padre.Nombre, Nodo.Nombre, Tabla);                    
                }
                else
                {
                    Resultado = this._Conexion.EscribirTabla(Nodo.Padre.Nombre, Nodo.Nombre, Tabla);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Resultado;
        }

        #endregion

        #region Dispose

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (this._Conexion != null)
                this._Conexion = null;

            if (_NodoActual != null)
                _NodoActual = null;

            if (NodoTablaActual != null)
                NodoTablaActual = null;

            if (BorrarCodigoAdministrado)
            {
                if (Nodos != null)
                {
                    foreach (NodoViewModel N in Nodos)
                    {
                        N.Dispose();
                    }

                    Nodos.Clear();
                    Nodos = null;
                }
            }
        }

        #endregion

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
