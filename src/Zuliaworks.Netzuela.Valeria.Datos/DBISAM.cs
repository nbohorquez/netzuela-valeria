namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                              // ConnectionState, DataTable
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Text;

    using Dbisam;
    using Zuliaworks.Netzuela.Valeria.Comunes;      // ParametrosDeConexion

    public partial class DBISAM : EventosComunes, IBaseDeDatosLocal
    {
        #region Variables y Constantes

        private const string ControladorOdbc = "DBISAM 4 ODBC Driver";
        private DbisamConnection conexion;

        #endregion

        #region Constructores

        public DBISAM(ParametrosDeConexion servidorBd)
        {
            this.DatosDeConexion = servidorBd;
            this.conexion = new DbisamConnection();
            this.conexion.StateChange -= this.ManejarCambioDeEstado;
            this.conexion.StateChange += this.ManejarCambioDeEstado;
        }

        ~DBISAM()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;

            if (BorrarCodigoAdministrado)
            {
                if (this.conexion != null)
                {
                    this.conexion.Dispose();
                    this.conexion = null;
                }
            }
        }

        private void CambiarBaseDeDatos(string baseDeDatos)
        {
            if (this.conexion.Database != baseDeDatos)
            {
                this.conexion.ChangeDatabase(baseDeDatos);
            }
        }

        private void EjecutarOrden(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            DbisamCommand orden = new DbisamCommand(sql, this.conexion);
            orden.ExecuteNonQuery();
        }

        private string[] LectorSimple(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            DbisamDataReader lector = null;
            List<string> resultado = new List<string>();
            DbisamCommand orden = new DbisamCommand(sql, this.conexion);
            lector = orden.ExecuteReader();

            while (lector.Read())
            {
                resultado.Add(lector.GetString(0));
            }

            lector.Close();
            return resultado.ToArray();
        }

        private DataTable LectorAvanzado(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            DataTable resultado = new DataTable();
            DbisamDataAdapter adaptador = new DbisamDataAdapter(sql, this.conexion);
            DbisamCommandBuilder creadorDeOrden = new DbisamCommandBuilder(adaptador);
            adaptador.FillSchema(resultado, SchemaType.Source);
            adaptador.Fill(resultado);
            
            return resultado;
        }

        private string DescribirTabla(string tabla)
        {
            return "*";
            /*
            DataTable descripcion = this.LectorAvanzado("DESCRIBE " + tabla);

            var columnasPermitidas = from D in descripcion.AsEnumerable()
                                     select D.Field<string>("Field");

            return string.Join(", ", columnasPermitidas.ToArray());
             */
        }

        /*
         * SEGURIDAD DE NOMBRE DE USUARIO Y CONTRASEÑA
         * ===========================================
         * 
         * Tengo que revisar la logica de las funciones usadas para crear la ruta de acceso al servidor.
         * Estoy haciendo que los datos que entran y salen de ambas funciones sean seguros (SecureString).
         * Sin embargo las operaciones internas las estoy haciendo con string normales (no es sino hasta el 
         * final que las convierto en SecureStrings) y no se si eso pueda suponer un "hueco" de seguridad.
         */

        private SecureString CrearRutaDeAcceso(ParametrosDeConexion seleccion, SecureString usuario, SecureString contrasena)
        {
            /*
             * Informacion tomada de: http://www.elevatesoft.com/manual?action=viewtopic&id=dbisam4odbc&topic=Connection_Strings
             * 
             * ConnectionType
             * ==============
             * This string value is set to either "Local" if the data source is accessing the database (also called a catalog) 
             * directly, or "Remote" if the data source is accessing the database remotely via a database server.
             * 
             * CatalogName
             * ===========
             * This string value is the name of the database, or catalog, being used for the data source. The name can be either 
             * a directory name, if the data source is configured for local access (ConnectionType="Local"), or a database name, 
             * if the data source is configured for remote access to a database server (ConnectionType="Remote").
             * 
             * ReadOnly
             * ========
             * This string value is set to "True" if the data source is read-only, and "False" if the data source is read-write.
             * 
             * UID
             * ===
             * This string value specifies the user ID to use for accessing a remote DBISAM database server. This value is only 
             * used when the ConnectionType registry value is set to "Remote". If this value is left blank, the user will be 
             * prompted for the user ID when accessing the database server.
             * 
             * PWD
             * ===
             * This string value specifies the password to use for accessing a remote DBISAM database server. This value is only 
             * used when the ConnectionType registry value is set to "Remote". If this value is left blank, the user will be 
             * prompted for the password when accessing the database server.
             * 
             * RemoteEncryption
             * ================
             * This string value controls whether the connection to a remote DBISAM database server will be encrypted. If it is "False" 
             * (the default), then the connection will not be encrypted. If it is "True", then the RemoteEncryptionPassword keyword (see below) 
             * will specify the password to use.
             * 
             * RemoteEncryptionPassword	
             * ========================
             * This string value specifies the password to use for encrypting all requests and responses to and from a remote DBISAM database 
             * server when the connection is encrypted (see RemoteEncryption keyword above). The default value is "elevatesoft".
             * 
             * RemoteHostName
             * ==============
             * This string value specifies the host name of the machine running the remote DBISAM database server that you are accessing. Either 
             * the RemoteHostName or RemoteIPAddress registry values must be populated along with the RemoteService or RemotePort registry values 
             * in order to correctly access a remote DBISAM database server. The default value is "".
             * 
             * RemoteIPAddress
             * ===============
             * This string value specifies the IP address of the machine running the remote DBISAM database server that you are accessing. Either 
             * the RemoteHostName or RemoteIPAddress registry values must be populated along with the RemoteService or RemotePort registry values 
             * in order to correctly access a remote DBISAM database server. The default value is "127.0.0.1".
             * 
             * RemoteService
             * =============
             * This string value specifies the service name of the remote DBISAM database server that you are accessing. Either the RemoteService 
             * or RemotePort registry values must be populated along with the RemoteHostName or RemoteIPAddress registry values in order to correctly 
             * access a remote DBISAM database server. The default value is "".
             * 
             * RemotePort
             * ==========
             * 
             * This string value specifies the port number of the remote DBISAM database server that you are accessing. Either the RemoteService or 
             * RemotePort registry values must be populated along with the RemoteHostName or RemoteIPAddress registry values in order to correctly 
             * access a remote DBISAM database server. The default value is "12005".
             * 
             * RemotePing
             * ==========
             * This string value controls whether pinging will be used to keep the connection to a remote DBISAM database server alive, even when the 
             * connection is inactive for long periods of time. If it is "False" (the default), then pinging will not be used. If it is "True", then 
             * the RemotePingInterval keyword (see below) will specify how often the pinging will occur.
             * 
             * RemotePingInterval
             * ==================
             * This string value specifies the interval (in seconds) to use when pinging has been enabled for the connection to a remote DBISAM database 
             * server (see RemotePing keyword above). The default value is "60" seconds.
             * 
             * TablePassword*
             * ==============
             * These string values are numbered as "TablePassword1", "TablePassword2", etc. and are used as passwords for opening encrypted tables.
             * 
             */

            /*
             * Usuario
             * =======
             * {"user"|"user id"}
             * 
             * Contraseña
             * ==========
             * {"pwd"|"password"}
             * 
             * Contraseña de encriptamiento
             * =====================
             * "encrypt password"
             * 
             * Encriptamiento
             * ============
             * {"encryption"|"is encrypted"}
             * 
             * Version del motor
             * =================
             * {"engine version"|"engine"}
             * 
             * Servidor
             * ========
             * {"host","data source","server"}
             * 
             * Base de datos
             * =============
             * {"database"|"initial catalog"}
             * 
             * Puerto
             * ======
             * "port"
             * 
             * Tiempo de vencimiento de conexion
             * =================================
             * {"connect timeout"|"connection timeout"}
             * 
             * Compresion
             * ==========
             * "compression"
             * 
             * Tamaño maximo del pool
             * ======================
             * "max pool size"
             * 
             * Tamaño minimo del pool
             * ======================
             * "min pool size"
             * 
             * Pooling
             * =======
             * "pooling"
             * 
             * Tiempo de vida de la conexion
             * =============================
             * "connection life time"
             * 
             * Intervalo del ping
             * ==================
             * "ping interval"
             * 
             */

            if (seleccion == null)
            {
                throw new ArgumentNullException("seleccion");
            }

            if (usuario == null)
            {
                throw new ArgumentNullException("usuario");
            }

            if (contrasena == null)
            {
                throw new ArgumentNullException("contrasena");
            }

            SecureString rutaDeConexion = new SecureString();

            /*
            // 1) Nombre del driver ODBC a emplear
            rutaDeConexion.AgregarString("DRIVER={" + DBISAM.ControladorOdbc + "};");
            
            // 2) Metodo de conexion
            switch (seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    IPAddress ip;
                    rutaDeConexion.AgregarString("ConnectionType=Remote;");
                    seleccion.Anfitrion = seleccion.Anfitrion.Equals("localhost") ? "127.0.0.1" : seleccion.Anfitrion;

                    // Aqui hay otra forma de validar una direccion IP: http://www.dreamincode.net/code/snippet1378.htm
                    if (IPAddress.TryParse(seleccion.Anfitrion, out ip))
                    {
                        rutaDeConexion.AgregarString("RemoteIPAddress=" + seleccion.Anfitrion + ";");
                        rutaDeConexion.AgregarString("RemotePort=" + seleccion.ArgumentoDeConexion + ";");
                    }
                    else
                    {
                        rutaDeConexion.AgregarString("RemoteHostName=" + seleccion.Anfitrion + ";");
                        rutaDeConexion.AgregarString("RemoteService=" + seleccion.ArgumentoDeConexion + ";");
                    }

                    rutaDeConexion.AgregarString("RemotePing=True;");
                    rutaDeConexion.AgregarString("RemotePingInterval=60;");
                    break;
                case MetodosDeConexion.Archivo:
                    rutaDeConexion.AgregarString("ConnectionType=Local;");
                    rutaDeConexion.AgregarString("CatalogName=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                default:
                    break;
            }

            //rutaDeConexion.AgregarString("CatalogName=prueba;");
            
            // 3) Nombre de usuario
            rutaDeConexion.AgregarString("UID=" + usuario.ConvertirAUnsecureString() + ";");

            // 4) Contraseña
            rutaDeConexion.AgregarString("PWD=" + contrasena.ConvertirAUnsecureString());

            */
            
            rutaDeConexion.AgregarString("engine=4;");

            switch (seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    rutaDeConexion.AgregarString("host=" + seleccion.Anfitrion + ";");
                    rutaDeConexion.AgregarString("port=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                default:
                    break;
            }
            
            // 3) Nombre de usuario
            rutaDeConexion.AgregarString("user=" + usuario.ConvertirAUnsecureString() + ";");

            // 4) Contraseña
            rutaDeConexion.AgregarString("pwd=" + contrasena.ConvertirAUnsecureString());

            return rutaDeConexion;
        }

        #endregion

        #region Implementacion de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return this.conexion.State; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion

        #region Funciones

        #region Métodos sincrónicos

        public void Conectar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                this.Desconectar();

                this.conexion.ConnectionString = this.CrearRutaDeAcceso(this.DatosDeConexion, usuario, contrasena).ConvertirAUnsecureString();
                this.conexion.Open();
            }
            catch (DbisamException ex)
            {
                throw new Exception("Error en la conexión.", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                if (this.conexion != null)
                {
                    this.conexion.Close();
                }
            }
            catch (DbisamException ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos.", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> resultadoFinal = null;

            try
            {
                /*string[] resultadoBruto = this.LectorSimple();
                resultadoFinal = new List<string>();
                
                foreach (string r in resultadoBruto)
                {
                    if (r != "information_schema" && r != "mysql" && r != "performance_schema")
                    {
                        resultadoFinal.Add(r);
                    }
                }
                 * */

                resultadoFinal = new List<string>() { "bd1", "bd2", "bd3" };
            }
            catch (DbisamException ex)
            {
                throw new Exception("Error al listar las bases de datos.", ex);
            }

            return resultadoFinal.ToArray();
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            List<string> resultado = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);
                /*
                string[] resultadoBruto = this.LectorSimple();
                resultado = new List<string>();

                foreach (string s in resultadoBruto)
                {
                    resultado.Add(s);
                }
                 */

                string[] resultadoBruto = new string[] { "tabla1", "tabla2", "tabla3" };
                resultado = new List<string>();

                foreach (string s in resultadoBruto)
                {
                    resultado.Add(s);
                }
            }
            catch (DbisamException ex)
            {
                throw new Exception("Error al listar las tablas.", ex);
            }

            return resultado.ToArray();
        }

        public DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            DataTable tablaLeida = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);
                /*
                 * Tenemos que ver primero cuales son las columnas a las que tenemos acceso. Un "SELECT * 
                 * FROM ..." podria generar un error si el usuario no tiene los privilegios suficientes.
                 */
                string columnas = this.DescribirTabla(tabla);
                tablaLeida = this.LectorAvanzado("SELECT " + columnas + " FROM " + tabla);
            }
            catch (DbisamException ex)
            {
                throw new Exception("Error al leer la tabla.", ex);
            }

            return tablaLeida;
        }

        public bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            throw new NotImplementedException();
        }

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            DataTable resultado = null;
            this.CambiarBaseDeDatos(baseDeDatos);

            try
            {
                resultado = this.LectorAvanzado(sql);
            }
            catch (DbisamException ex)
            {
                throw new Exception("No se pudo realizar la consulta.", ex);
            }

            return resultado;
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
