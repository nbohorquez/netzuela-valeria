namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;

    public partial class DBISAM
    {
        #region Constantes

        public const string PuertoPrincipal = "12005";
        public const string PuertoAdministrativo = "12006";

        #endregion

        #region Funciones

        public static ServidorLocal DetectarServidor()
        {
            ServidorLocal.MetodoDeConexion metodo = new ServidorLocal.MetodoDeConexion()
            {
                Nombre = MetodosDeConexion.TcpIp,
                Valores = new List<string>() { DBISAM.PuertoPrincipal, DBISAM.PuertoAdministrativo }
            };

            ServidorLocal.Instancia instancia = new ServidorLocal.Instancia() 
            { 
                Nombre = "Por defecto",
                Metodos = new List<ServidorLocal.MetodoDeConexion>() { metodo }
            };

            ServidorLocal serv = new ServidorLocal()
            {
                Nombre = RDBMS.DbIsam,
                Instancias = new List<ServidorLocal.Instancia>() { instancia }
            };

            return serv;
        }

        #endregion
    }
}
