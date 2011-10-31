using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                      // ConnectionState, DataTable
using System.Security;                  // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;                  // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos de Netzuela en Internet
    /// </summary>
    public class Netzuela : IBaseDeDatos
    {
        #region Variables

        // ¡Temporal!
        private List<Nodito> ServidorRemoto;
        ConnectionState Estado;
        public DatosDeConexion Servidor;

        #endregion

        #region Constructores

        public Netzuela(DatosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            
            // Me invento una base de datos ficticia
            ServidorRemoto = new List<Nodito>()
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

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        // ...

        #endregion

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return Estado; }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena) 
        {
            Estado = ConnectionState.Open;
        }

        void IBaseDeDatos.Desconectar() 
        {
            Estado = ConnectionState.Closed;
        }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            List<string> Resultado = new List<string>();

            foreach (Nodito N in ServidorRemoto[0].Hijos)
                Resultado.Add(N.Nombre);

            return Resultado.ToArray();
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = new List<string>();

            Nodito BD = Nodito.BuscarNodo(BaseDeDatos, ServidorRemoto[0].Hijos);

            foreach (Nodito N in BD.Hijos)
                Resultado.Add(N.Nombre);

            return Resultado.ToArray();
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            
            DataTable Tbl = new DataTable();

            Nodito BD = Nodito.BuscarNodo(BaseDeDatos, ServidorRemoto[0].Hijos);
            Nodito T = Nodito.BuscarNodo(Tabla, BD.Hijos);

            foreach (Nodito N in T.Hijos)
                Tbl.Columns.Add(N.Nombre);

            return Tbl;
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
