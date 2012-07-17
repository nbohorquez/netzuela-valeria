namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;               // ObservableCollection
    using System.Data;                                  // DataTable
    using System.Linq;
    using System.Text;
    using System.Windows;                               // MessageBox
    
    using Zuliaworks.Netzuela.Valeria.Comunes;          // Constantes, ExpresionGenerica
    using Zuliaworks.Netzuela.Valeria.Datos;            // EventoEnviarTablasCompletadoArgs
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    
    public partial class ExploradorViewModel : IDisposable
    {
        #region Funciones

        #region Otras

        private void AsignarEsteExploradorA(ObservableCollection<NodoViewModel> nodos)
        {
            foreach (NodoViewModel nodo in nodos)
            {
                nodo.Explorador = this;
                if (nodo.Hijos.Count > 0)
                {
                    this.AsignarEsteExploradorA(nodo.Hijos);
                }
            }
        }

        #endregion

        #region Acciones

        protected void EstablecerNodoActualAccion(string nombre)
        {
            try
            {
                this.NodoActual = (nombre == null)
                    ? this.NodoActual
                    : NodoViewModelExtensiones.BuscarNodo(nombre, NodoTablaActual.Hijos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        protected void ExpandirAccion(NodoViewModel nodo)
        {
            try
            {
                this.Expandir(nodo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        #endregion

        #region Expandir

        private void ExpandirServidor(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.Expandido == true)
            {
                return;
            }

            EventHandler<EventoListarBDsCompletadoArgs> retorno = null;
            string[] basesDeDatos = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (basesDeDatos != null)
                {
                    item.Expandido = true;

                    var eliminar = from hijo in item.Hijos
                                   where !(from bd in basesDeDatos
                                           select bd).Contains(hijo.Nombre)
                                   select hijo;

                    var agregar = from bd in basesDeDatos
                                  where !(from hijo in item.Hijos
                                          select hijo.Nombre).Contains(bd)
                                  select bd;

                    foreach (var hijo in eliminar)
                    {
                        hijo.Dispose();
                    }

                    foreach (var bd in agregar)
                    {
                        NodoViewModel n = new NodoViewModel(bd, item);
                    }
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            retorno = (r, a) =>
            {
                // Esto va metido entre try/cath porque se ejecuta solo
                try
                {
                    conexion.ListarBasesDeDatosCompletado -= retorno;

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
                        basesDeDatos = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (this.OperacionAsincronica)
            {
                this.conexion.ListarBasesDeDatosCompletado -= retorno;
                this.conexion.ListarBasesDeDatosCompletado += retorno;
                this.conexion.ListarBasesDeDatosAsinc();
            }
            else
            {
                basesDeDatos = this.conexion.ListarBasesDeDatos();
                CrearNodos();
            }
        }

        private void ExpandirBaseDeDatos(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.Expandido == true)
            {
                return;
            }

            EventHandler<EventoListarTablasCompletadoArgs> retorno = null;
            string[] tablas = null;

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (tablas != null)
                {
                    item.Expandido = true;

                    var eliminar = from hijo in item.Hijos
                                   where !(from tabla in tablas
                                           select tabla).Contains(hijo.Nombre)
                                   select hijo;

                    var agregar = from tabla in tablas
                                  where !(from hijo in item.Hijos
                                          select hijo.Nombre).Contains(tabla)
                                  select tabla;

                    foreach (var hijo in eliminar)
                    {
                        hijo.Dispose();
                    }

                    foreach (var tabla in agregar)
                    {
                        NodoViewModel n = new NodoViewModel(tabla, item);
                    }
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            retorno = (r, a) =>
            {
                try
                {
                    this.conexion.ListarTablasCompletado -= retorno;

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
                        tablas = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (this.OperacionAsincronica)
            {
                this.conexion.ListarTablasCompletado -= retorno;
                this.conexion.ListarTablasCompletado += retorno;
                this.conexion.ListarTablasAsinc(item.Nombre);
            }
            else
            {
                tablas = this.conexion.ListarTablas(item.Nombre);
                CrearNodos();
            }
        }

        private void ExpandirTabla(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Item");
            }

            DataTable tabla = null;
            EventHandler<EventoLeerTablaCompletadoArgs> retorno = null;

            ExpresionGenerica AjustarExplorador = () =>
            {
                item.Expandido = true;

                /* 
                    * NodoTablaActual esta atado a TablaActual: el primero es el indice dentro del 
                    * diccionario Tablas para ubicar el segundo.
                    */

                this.NodoTablaActual = item;
                this.TablaActual = tabla;
            };

            // Esta expresion lambda es llamada mas abajo.
            ExpresionGenerica CrearNodos = () =>
            {
                if (tabla != null)
                {
                    //Tabla.TableName = Item.RutaCompleta();
                    tabla.TableName = item.Nombre;

                    var eliminar = from hijo in item.Hijos
                                   where !(from columna in tabla.Columns.Cast<DataColumn>()
                                           select columna.ColumnName).Contains(hijo.Nombre)
                                   select hijo;

                    var agregar = from columna in tabla.Columns.Cast<DataColumn>()
                                  where !(from hijo in item.Hijos
                                          select hijo.Nombre).Contains(columna.ColumnName)
                                  select columna;

                    foreach (var hijo in eliminar)
                    {
                        hijo.Dispose();
                    }

                    foreach (var columna in agregar)
                    {
                        NodoViewModel n = new NodoViewModel(columna.ColumnName, item);
                    }
                                        
                    AjustarExplorador();
                }
            };

            // Este es el retorno en caso de hacerse una llamada asincronica
            retorno = (r, a) =>
            {
                try
                {
                    this.conexion.LeerTablaCompletado -= retorno;

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
                        tabla = a.Resultado;
                        CrearNodos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            if (item.ExisteEnRepositorioDeTablas())
            {
                tabla = item.BuscarEnRepositorioDeTablas();
                AjustarExplorador();
            }
            else
            {
                if (this.OperacionAsincronica)
                {
                    this.conexion.LeerTablaCompletado -= retorno;
                    this.conexion.LeerTablaCompletado += retorno;
                    this.conexion.LeerTablaAsinc(item.Padre.Nombre, item.Nombre);
                }
                else
                {
                    tabla = this.conexion.LeerTabla(item.Padre.Nombre, item.Nombre);
                    CrearNodos();
                }
            }
        }

        private void ExpandirColumna(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            NodoViewModel padre = item.Padre;
            this.ExpandirTabla(padre);

            item.Expandido = true;
        }

        public void ExpandirRuta(string ruta)
        {
            string[] pasosDeLaRuta = ruta.Split('\\');

            ObservableCollection<NodoViewModel> lista = Nodos;
            NodoViewModel nodo = null;

            try
            {
                foreach (string paso in pasosDeLaRuta)
                {
                    if (paso == string.Empty)
                    {
                        continue;
                    }

                    nodo = NodoViewModelExtensiones.RutaANodo(paso, lista);

                    if (nodo != null)
                    {
                        this.Expandir(nodo);
                        lista = nodo.Hijos;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al expandir la ruta: " + ruta, ex);
            }
        }

        /// <summary>
        /// Expande todos los nodos del árbol de nodos <see cref="Nodos"/> de este explorador.
        /// </summary>
        public void ExpandirTodo()
        {
            try
            {
                this.ExpandirTodo(this.Nodos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Expande todos los nodos del árbol de nodos especificado.
        /// </summary>
        /// <param name="nodos">Arbol a expandir.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="nodos"/> es una referencia 
        /// nula.</exception>
        public void ExpandirTodo(ObservableCollection<NodoViewModel> nodos)
        {
            if (nodos == null)
            {
                throw new ArgumentNullException("Nodos");
            }

            try
            {
                // Este mismo ciclo no quiso funcionar con un foreach porque se perdia la numeracion
                for (int i = 0; i < nodos.Count; i++)
                {
                    this.Expandir(nodos[i]);

                    if (nodos[i].Hijos.Count > 0)
                    {
                        this.ExpandirTodo(nodos[i].Hijos);
                    }
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
        /// <param name="item">Nodo a expandir. El nivel de este nodo solo puede ser uno de los 
        /// especificados en <see cref="Constantes.NivelDeNodo"/>.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="item"/> es una referencia 
        /// nula.</exception>
        public void Expandir(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Item");
            }

            try
            {
                switch (item.Nivel)
                {
                    case NivelDeNodo.Servidor:
                        this.ExpandirServidor(item);
                        break;
                    case NivelDeNodo.BaseDeDatos:
                        this.ExpandirBaseDeDatos(item);
                        break;
                    case NivelDeNodo.Tabla:
                        this.ExpandirTabla(item);
                        break;
                    case NivelDeNodo.Columna:
                        this.ExpandirColumna(item);
                        break;
                    default:
                        break;
                }

                this.NodoActual = item;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al expandir el NodoViewModel " + item.Nombre, ex); 
            }
        }

        public void Reexpandir(NodoViewModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Item");
            }

            item.Expandido = false;

            if (item.ExisteEnRepositorioDeTablas())
            {
                item.QuitarDeRepositorioDeTablas();
            }

            this.Expandir(item);
        }

        #endregion

        #region Funciones de tablas

        public bool EscribirTabla(NodoViewModel nodo, DataTable tabla)
        {
            if (nodo.Nivel != NivelDeNodo.Tabla)
            {
                return false;
            }

            bool resultado = false;
            EventHandler<EventoEscribirTablaCompletadoArgs> retorno = null;

            retorno = (r, a) =>
            {
                // Esto va metido entre try/cath porque se ejecuta solo
                try
                {
                    bool re = false;
                    string mensaje = string.Empty;

                    this.conexion.EscribirTablaCompletado -= retorno;

                    if (a.Error != null)
                    {
                        throw a.Error;
                    }
                    else if (a.Cancelled)
                    {
                        mensaje += "Operacion EscribirTablaAsinc cancelada";
                    }
                    else
                    {
                        re = a.Resultado;
                        mensaje += "El resultado de la operacion EscribirTablaAsinc fue: " + re.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
            };

            try
            {
                if (this.OperacionAsincronica)
                {
                    //this.conexion.EscribirTablaCompletado -= retorno;
                    //this.conexion.EscribirTablaCompletado += retorno;
                    this.conexion.EscribirTablaAsinc(nodo.Padre.Nombre, nodo.Nombre, tabla);                    
                }
                else
                {
                    resultado = this.conexion.EscribirTabla(nodo.Padre.Nombre, nodo.Nombre, tabla);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultado;
        }

        #endregion

        #region Dispose

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (this.conexion != null)
                this.conexion = null;

            if (nodoActual != null)
                nodoActual = null;

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
