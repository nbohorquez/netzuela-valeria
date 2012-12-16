namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public interface IBaseDeDatosLocal : IBaseDeDatos
    {
        #region Prototipos de funciones

        #region Métodos sincrónicos

        DataTable LeerTabla(string baseDeDatos, string tabla);
        bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla);

        #endregion

        #region Métodos asincrónicos

        void LeerTablaAsinc(string baseDeDatos, string tabla);
        void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla);

        #endregion

        #endregion
    }
}