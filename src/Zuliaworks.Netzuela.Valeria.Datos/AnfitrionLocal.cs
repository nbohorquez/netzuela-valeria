namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal

    /// <summary>
    /// Contiene el esqueleto de los parámetros de conexión con los servidores de bases de datos.
    /// </summary>    
    public static class AnfitrionLocal
    {
        #region Funciones

        public static List<ServidorLocal> DetectarTodosLosServidores()
        {
            List<ServidorLocal> Servidores = new List<ServidorLocal>();

            Servidores.Add(SQLServer.DetectarServidor());
            Servidores.Add(Oracle.DetectarServidor());
            Servidores.Add(MySQL.DetectarServidor());

            return Servidores;
        }

        #endregion
    }
}
