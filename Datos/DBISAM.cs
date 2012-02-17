using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                              // ConnectionState, DataTable
using Zuliaworks.Netzuela.Valeria.Comunes;      // ParametrosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    public class DBISAM : EventosComunes, IBaseDeDatos
    {
        #region Constructores

        ~DBISAM()
        {
            Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (BorrarCodigoAdministrado)
            {
            }
        }

        #endregion

        #region Implementacion de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { throw new NotImplementedException(); }
        }

        public ParametrosDeConexion DatosDeConexion
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Funciones

        #region Métodos sincrónicos

        public void Conectar(System.Security.SecureString Usuario, System.Security.SecureString Contrasena)
        {
            throw new NotImplementedException();
        }

        public void Desconectar()
        {
            throw new NotImplementedException();
        }

        public string[] ListarBasesDeDatos()
        {
            throw new NotImplementedException();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            throw new NotImplementedException();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            throw new NotImplementedException();
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, System.Data.DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public bool CrearUsuario(System.Security.SecureString Usuario, System.Security.SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            throw new NotImplementedException();
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            throw new NotImplementedException();
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            throw new NotImplementedException();
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, System.Data.DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public void CrearUsuarioAsinc(System.Security.SecureString Usuario, System.Security.SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        public void ConsultarAsinc(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
