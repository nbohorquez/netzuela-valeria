using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Contiene el esqueleto de los parámetros de conexión con los servidores de bases de datos.
    /// </summary>    
    public class ServidorLocal
    {
        #region Variables

        // ...

        #endregion

        #region Constructores

        public ServidorLocal() { }

        #endregion

        #region Propiedades

        // Un servidor puede crear muchas instancias de si mismo
        public string Nombre { get; set; }
        public List<Instancia> Instancias { get; set; }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        public static List<ServidorLocal> DetectarTodos()
        {
            List<ServidorLocal> Servidores = new List<ServidorLocal>();

            Servidores.Add(SQLServer.DetectarServidor());
            Servidores.Add(Oracle.DetectarServidor());
            Servidores.Add(MySQL.DetectarServidor());

            return Servidores;
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        public struct Instancia
        {
            // Toda instancia tiene nombre y uno o mas metodos de conexion
            public string Nombre { get; set; }
            public List<MetodoDeConexion> Metodos { get; set; }
        }

        public struct MetodoDeConexion
        {
            // Los metodos de conexion soportados estan definidos en Constantes.cs
            public string Nombre { get; set; }
            public List<string> Valores { get; set; }
        }

        #endregion
    }
}
