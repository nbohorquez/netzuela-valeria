namespace Zuliaworks.Netzuela.Valeria.Cliente.Logica
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;        // ServidorLocal
    using Zuliaworks.Netzuela.Valeria.Datos;          // AnfitrionLocal

    public static class ExponerAnfitrionLocal
    {
        /// <summary>
        /// Obtiene los detalles de conexión de todos los servidores detectados en el equipo local.
        /// </summary>
        /// <returns></returns>
        public static List<ServidorLocal> DetectarServidoresLocales()
        {
            return AnfitrionLocal.DetectarTodosLosServidores();
        }
    }
}
