using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal, ParametrosDeConexion
using System.Data;                                  // DataTable
using System.Data.SqlClient;                        // SqlConnection
using System.Security;                              // SecureString

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos SQLServer
    /// </summary>
    public partial class SQLServer : IBaseDeDatos
    {
        #region Variables
                
        private SqlConnection _Conexion;

        #endregion

        #region Constructores

        public SQLServer(ParametrosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            _Conexion = new SqlConnection();
        }

        #endregion

        #region Propiedades

        public ParametrosDeConexion Servidor { get; set; }

        #endregion

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return _Conexion.State; }
        }

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set { _Conexion.StateChange += value; }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena) { }

        void IBaseDeDatos.Desconectar() { }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        object IBaseDeDatos.CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
