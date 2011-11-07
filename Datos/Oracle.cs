using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Security;          // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos Oracle
    /// </summary>
    public class Oracle : IBaseDeDatos
    {
        #region Variables

        public DatosDeConexion Servidor;

        #endregion

        #region Constructores

        public Oracle(DatosDeConexion ServidorBD) 
        {
            Servidor = ServidorBD;
        }

        #endregion

        #region Propiedades

        // ...

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        public static ServidorLocal DetectarServidor()
        {
            /*
             * Oracle:
             * ======
             * 
             * Por hacer...
             */

            List<ServidorLocal.Instancia> Instancias = new List<ServidorLocal.Instancia>();

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.ORACLE;
            Serv.Instancias = Instancias;
            return Serv;
        }

        #endregion

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return ConnectionState.Closed; }
        }

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set { throw new NotImplementedException(); }
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

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
