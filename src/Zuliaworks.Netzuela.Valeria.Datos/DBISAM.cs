namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                              // ConnectionState, DataTable
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;      // ParametrosDeConexion

    public class DBISAM : EventosComunes, IBaseDeDatosLocal
    {
        #region Constructores

        ~DBISAM()
        {
            this.Dispose(false);
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

        public string[] ListarTablas(string baseDeDatos)
        {
            throw new NotImplementedException();
        }

        public DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            throw new NotImplementedException();
        }

        public bool EscribirTabla(string baseDeDatos, string nombreTabla, System.Data.DataTable tabla)
        {
            throw new NotImplementedException();
        }

        public bool CrearUsuario(System.Security.SecureString usuario, System.Security.SecureString contrasena, string[] columnas, int privilegios)
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

        public void ListarTablasAsinc(string baseDeDatos)
        {
            throw new NotImplementedException();
        }

        public void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
            throw new NotImplementedException();
        }

        public void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, System.Data.DataTable tabla)
        {
            throw new NotImplementedException();
        }

        public void CrearUsuarioAsinc(System.Security.SecureString usuario, System.Security.SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public void ConsultarAsinc(string baseDeDatos, string sql)
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

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
