namespace Zuliaworks.Netzuela.Valeria.Datos {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security;                  // SecureString
    using System.Text;

    public interface IBaseDeDatosLocal : IBaseDeDatos {
        #region Prototipos de funciones

        #region Métodos sincrónicos

        DataTable LeerTabla(string baseDeDatos, string tabla);
        bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla);
        bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios);
        DataTable Consultar(string baseDeDatos, string sql);

        #endregion

        #region Métodos asincrónicos

        void LeerTablaAsinc(string baseDeDatos, string tabla);
        void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla);
        void ConsultarAsinc(string baseDeDatos, string sql);
        void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios);

        #endregion

        #endregion
    }
}