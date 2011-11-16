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
        #region Tipos anidados

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

        public class Privilegios
        {
            public const int NO_VALIDO = 0;
            public const int SELECCIONAR = 1<<0;
            public const int INSERTAR_FILAS = 1<<1;
            public const int ACTUALIZAR = 1<<2;
            public const int BORRAR_FILAS = 1<<3;
            public const int INDIZAR = 1<<4;
            public const int ALTERAR = 1<<5;
            public const int CREAR = 1<<6;
            public const int DESTRUIR = 1<<7;
        }

        #endregion
    }
}
