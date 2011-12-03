using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // ConnectionState, DataTable
using System.Security;                          // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos de Netzuela en Internet
    /// </summary>
    public class Netzuela : IBaseDeDatos
    {
        #region Variables

        // ¡Temporal!
        private List<Nodito> _ServidorRemoto;
        private ConnectionState _Estado;
        
        #endregion

        #region Constructores

        public Netzuela(ParametrosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            _Estado = ConnectionState.Closed;

            // Me invento una base de datos ficticia
            _ServidorRemoto = new List<Nodito>()
            {
                new Nodito() 
                { 
                    Nombre = "Netzuela", 
                    Hijos = new List<Nodito>() 
                    {
                        new Nodito()
                        {
                            Nombre = "Spuria",
                            Hijos = new List<Nodito>()
                            {
                                new Nodito()
                                {
                                    Nombre = "Ordenes de compra",
                                    Hijos = new List<Nodito>()
                                    {
                                        new Nodito() { Nombre = "Codigo", Hijos = null },
                                        new Nodito() { Nombre = "Cantidad de articulos", Hijos = null },
                                        new Nodito() { Nombre = "Total", Hijos = null }
                                    }
                                },
                                new Nodito()
                                {
                                    Nombre = "Inventario",
                                    Hijos = new List<Nodito>()
                                    {
                                        new Nodito() { Nombre = "Codigo", Hijos = null },
                                        new Nodito() { Nombre = "Descripcion", Hijos = null },
                                        new Nodito() { Nombre = "Cantidad", Hijos = null },
                                        new Nodito() { Nombre = "Precio", Hijos = null }
                                    }
                                }
                            }
                        }
                    }
                }
            };

                /*
            ServidorRemoto = new List<Nodito>()
            {
                { new Nodo("Netzuela", Constantes.NivelDeNodo.SERVIDOR) }
            };

            ServidorRemoto[0].Hijos.Clear();
            Nodo Spuria = new Nodo("Spuria", ServidorRemoto[0]);

            Spuria.Hijos.Clear();
            Nodo Inventario = new Nodo("Inventario", Spuria, new string[] { "Codigo", "Descripcion", "Precio", "Cantidad" });
                 * */
        }

        #endregion

        #region Propiedades

        public ParametrosDeConexion Servidor { get; set; }

        #endregion

        #region Eventos

        public event StateChangeEventHandler Cambio;

        #endregion

        #region Implementaciones de interfaces
        
        public ConnectionState Estado
        {
            get { return _Estado; }
            private set
            {
                if(value != _Estado)
                {
                    ConnectionState Anterior = _Estado;
                    _Estado = value;
                    Cambio(this, new StateChangeEventArgs(Anterior, _Estado));
                }
            }
        }

        public StateChangeEventHandler EnCambioDeEstado
        {
            set { Cambio += value; }
        }

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            this.Estado = ConnectionState.Open;
        }

        public void Desconectar() 
        {
            Estado = ConnectionState.Closed;
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> Resultado = new List<string>();

            foreach (Nodito N in _ServidorRemoto[0].Hijos)
                Resultado.Add(N.Nombre);

            return Resultado.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = new List<string>();

            Nodito BD = Nodito.BuscarNodo(BaseDeDatos, _ServidorRemoto[0].Hijos);

            foreach (Nodito N in BD.Hijos)
                Resultado.Add(N.Nombre);

            return Resultado.ToArray();
        }

        public DataTable MostrarTabla(string BaseDeDatos, string Tabla)
        {
            
            DataTable Tbl = new DataTable();

            Nodito BD = Nodito.BuscarNodo(BaseDeDatos, _ServidorRemoto[0].Hijos);
            Nodito T = Nodito.BuscarNodo(Tabla, BD.Hijos);

            foreach (Nodito N in T.Hijos)
                Tbl.Columns.Add(N.Nombre);

            return Tbl;
        }

        public object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Tipos anidados

        private class Nodito
        {
            public string Nombre;
            public List<Nodito> Hijos;

            public Nodito() { }

            public static Nodito BuscarNodo(string Nombre, List<Nodito> Lista)
            {
                Nodito Resultado = new Nodito();

                foreach (Nodito n in Lista)
                {
                    if (n.Nombre == Nombre)
                    {
                        Resultado = n;
                        break;
                    }
                }

                return Resultado;
            }
        }

        #endregion
    }
}
