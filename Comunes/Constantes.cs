using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    /// <summary>
    /// Define constantes empleadas por todas las capas de Valeria
    /// </summary>    
    public static class Constantes
    {
        /// <summary>
        /// SGBDR: Sistemas Gestores de Bases de Datos Relacionales (RDBMS en inglés)
        /// </summary>
        public class SGBDR
        {
            public const string NETZUELA = "Netzuela";          // No es realmente un SGBDR pero...
            public const string SQL_SERVER = "SQL Server";
            public const string MYSQL = "MySQL";
            public const string ORACLE = "Oracle";
            public const string ACCESS = "Access";
            public const string DB2 = "DB2";
        }

        /// <summary>
        /// Métodos de conexión
        /// </summary>        
        public class MetodosDeConexion
        {
            public const string MEMORIA_COMPARTIDA = "Memoria compartida";
            public const string CANALIZACIONES_CON_NOMBRE = "Canalizaciones con nombre";
            public const string TCP_IP = "TCP/IP";
            public const string VIA = "VIA";
        }

        /// <summary>
        /// Credenciales
        /// </summary>
        public class Credenciales
        {
            public const int CHIVO = 0;
            public const int NETZUELA = 1;
        }

        /// <summary>
        /// Credenciales
        /// </summary>
        public class NivelDeNodo
        {
            public const int NULO = -1;
            public const int RAIZ = 0;

            public const int SERVIDOR = 0;
            public const int BASE_DE_DATOS = 1;
            public const int TABLA = 2;
            public const int COLUMNA = 3;
        }
    }
}
