using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;              // DataTable, ConnectionState
using System.Security;          // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Esta clase se carga cuando no se ha podido conectar con otro proveedor de acceso.
    /// </summary>
    public class ServidorPredeterminado : IBaseDeDatos
    {
        #region Variables

        public DatosDeConexion Servidor;
        ConnectionState Estado;

        #endregion

        #region Constructores

        public ServidorPredeterminado(DatosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            Estado = ConnectionState.Closed;
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

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set { throw new NotImplementedException(); }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena) { }

        void IBaseDeDatos.Desconectar() { }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            return new string[] { };
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            return new string[] { };
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
