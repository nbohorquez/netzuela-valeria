using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal, ParametrosDeConexion
using System.Data;                                  // DataTable
using System.Data.SqlClient;                        // SqlConnection
using System.Security;                              // SecureString

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos SQLServer
    /// </summary>
    public partial class SQLServer : EventosComunes, IBaseDeDatos
    {
        #region Variables
                
        private SqlConnection _Conexion;

        #endregion

        #region Constructores

        public SQLServer(ParametrosDeConexion ServidorBD)
        {
            DatosDeConexion = ServidorBD;

            _Conexion = new SqlConnection();

            // Registramos el manejador de eventos por defecto. Este sirve como repetidor del evento subyacente.
            _Conexion.StateChange += new StateChangeEventHandler(ManejarCambioDeEstado);
        }

        #endregion

        #region Funciones

        private void CambiarBaseDeDatos(string BaseDeDatos)
        {
            try
            {
                if (_Conexion.Database != BaseDeDatos)
                    _Conexion.ChangeDatabase(BaseDeDatos);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al cambiar la base de datos. Error MSSQL No. " + ex.Number.ToString(), ex);
            }
        }

        private void EjecutarOrden(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            try
            {
                SqlCommand Orden = new SqlCommand(SQL, _Conexion);
                Orden.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo ejecutar la orden. Error MSSQL No. " + ex.Number.ToString(), ex);
            }
        }

        private string[] LectorSimple(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            SqlDataReader Lector = null;
            List<string> Resultado = new List<string>();

            try
            {
                SqlCommand Orden = new SqlCommand(SQL, _Conexion);

                Lector = Orden.ExecuteReader();
                while (Lector.Read())
                {
                    Resultado.Add(Lector.GetString(0));
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo obtener la lista de elementos desde la base de datos. Error MSSQL No. " + ex.Number.ToString(), ex);
            }
            finally
            {
                if (Lector != null)
                    Lector.Close();
            }

            return Resultado.ToArray();
        }

        private DataTable LectorAvanzado(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            DataTable Resultado = new DataTable();

            try
            {
                SqlDataAdapter Adaptador = new SqlDataAdapter(SQL, _Conexion);
                SqlCommandBuilder CreadorDeOrden = new SqlCommandBuilder(Adaptador);

                Adaptador.Fill(Resultado);
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo obtener la tabla la base de datos. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        private string RutaServidorFormatoTCPIP(ParametrosDeConexion Seleccion)
        {
            List<string> RutaDeConexion = new List<string>();
            
            switch (Seleccion.MetodoDeConexion)
            {
                case Constantes.MetodosDeConexion.TCP_IP:
                    RutaDeConexion.Add("tcp:");
                    break;
                case Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA:
                    RutaDeConexion.Add("lpc:");
                    break;
                case Constantes.MetodosDeConexion.VIA:
                    RutaDeConexion.Add("via:");
                    break;
                default:
                    throw new Exception("No se reconoce el metodo de conexion: \"" + Seleccion.MetodoDeConexion + "\"");
            }

            RutaDeConexion.Add((Seleccion.Anfitrion == "localhost") ? "." : Seleccion.Anfitrion);
            RutaDeConexion.Add("\\" + Seleccion.Instancia);

            if (Seleccion.ArgumentoDeConexion != null && Seleccion.ArgumentoDeConexion != string.Empty && Seleccion.ArgumentoDeConexion != "Por defecto")
                RutaDeConexion.Add("," + Seleccion.ArgumentoDeConexion);

            return string.Join(string.Empty, RutaDeConexion.ToArray());
        }

        private string RutaServidorFormatoCanalizaciones(ParametrosDeConexion Seleccion)
        {
            if(Seleccion.MetodoDeConexion != Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE)
                throw new Exception("No se reconoce el metodo de conexion: \"" + Seleccion.MetodoDeConexion + "\"");

            return "np:" + Seleccion.ArgumentoDeConexion;;
        }

        private SecureString CrearRutaDeAcceso(ParametrosDeConexion Seleccion, SecureString Usuario, SecureString Contrasena)
        {
            /*
             * La lista completa de las opciones de la ruta de conexion ("Connection String" en ingles) se detalla en:
             * http://msdn.microsoft.com/es-es/library/system.data.sqlclient.sqlconnection.connectionstring%28v=VS.90%29.aspx
             * 
             * NOMBRE DE LA APLICACION
             * =======================
             * "Application Name=", "App="
             * ---------------------------
             * "Application Name=('.NET SQLClient Data Provider');"
             * 
             * Nombre de la aplicacion. Puede ser de hasta 128 caracteres.
             * Por defecto es '.NET SQLClient Data Provider'
             * 
             * INTENCION DE LA APLICACION
             * ==========================
             * "ApplicationIntent="
             * --------------------
             * "ApplicationIntent=(ReadWrite);"
             * 
             * Declara el tipo de ordenes que sera posible ejecutar sobre la base de datos. 
             * Por defecto es ReadWrite. Las opciones son:
             * 
             *      *- ReadOnly: solo lectura.
             *      *- ReadWrite: lectura/escritura.
             * 
             * SERVIDOR
             * ========
             * "Data Source=", "Server=", "Address=", "Addr=", "Network Address="
             * ------------------------------------------------------------------
             * "Data Source=();"
             * 
             * Nombre del anfitrion o la direccion ip. Existen dos formatos para definir
             * este parametro:
             * 
             *      1) Formato TCP/IP:
             *      
             *      tcp:<host name>\<instance name>
             *      tcp:<host name>,<TCP/IP port number>
             *      
             *      El formato TCP/IP debe comenzar con el prefijo "tcp:" seguido de la instancia de la 
             *      base de datos a utilizar. Esta instancia se define como el nombre del anfitrion (<host name>) 
             *      mas el nombre de una instancia (<instance name>). El nombre de la instancia luego se
             *      resolvera como un numero de puerto. De igual forma, se puede especificar el numero de 
             *      puerto directamente. Si no se especifica ni nombre de instancia ni puerto, el sistema
             *      empleara la instancia definida por defecto.
             *      
             *      <host name> debe ser especificado como: NetBIOSName, IPv4Address o IPv6Address.
             * 
             *      2) Formato Canalizaciones:
             *      
             *      np:\\<host name>\pipe\<pipe name>
             *      
             *      <host name> debe ser especificado como: NetBIOSName, IPv4Address o IPv6Address.
             * 
             * Cuando se conecte al servidor local use siempre (local). Ej: np:(local), np:., 
             * tcp:(local), tcp:., lpc:(local), lpc:.
             * 
             *      *- tcp: TCP/IP
             *      *- lpc: Memoria compartida
             *      *- np: Canalizaciones
             *      *- via: VIA
             * 
             * Nota: Si se emplea la opcion "Network", no se deben especificar "tcp:" o "np:"
             * 
             * BIBLIOTECA DE RED
             * =================
             * "Network Library=", "Network=", "Net="
             * --------------------------------------
             * "Network Library=();"
             * 
             * Biblioteca empleada para establecer la conexion con la instancia del servidor de bases de datos.
             * Las opciones son:
             * 
             *      *- dbnmpntw: canalizaciones (Named Pipes)
             *      *- dbmsrpcn: multiprotocolo, RPC (Multiprotocol, Windows RPC)
             *      *- dbmsadsn: Apple Talk
             *      *- dbmsgnet: VIA
             *      *- dbmslpcn: memoria compartida (Shared Memory)
             *      *- dbmsspxn: IPX/SPX
             *      *- dbmssocn: TCP/IP
             *      *- Dbmsvinn: Banyan Vines
             * 
             * BASE DE DATOS
             * =============
             * "Initial Catalog=", "Database="
             * -------------------------------
             * "Database=();"
             * 
             * Nombre de la base de datos inicial a utilizar. Puede ser de hasta 128 caracteres.
             * 
             * USUARIO
             * =======
             * "User ID=", "UID="
             * ------------------
             * "UID=();"
             * 
             * Nombre de usuario/cuenta empleado en el proceso de autentificacion, puede ser de hasta 128 
             * caracteres. NO RECOMENDADO, emplee la opcion "Integrated Security" o "Trusted_Connection".
             * 
             * CONTRASEÑA
             * ==========
             * "Password=", "PWD="
             * -------------------
             * "PWD=();"
             * 
             * Contraseña asociada al usuario, puede ser de hasta 128 caracteres. NO RECOMENDADO, emplee la 
             * opcion "Integrated Security" o "Trusted_Connection".
             * 
             * CONEXION CONFIABLE
             * ==================
             * "Integrated Security=", "Trusted_Connection="
             * ---------------------------------------------
             * "Trusted_Connection=(false);"
             * 
             * Indica la forma en la que se hara la autentificacion de la conexion. Las opciones son:
             * 
             *      *- "true", "sspi": los credenciales actuales de Windows son empleados para autentificacion
             *      *- "false": el usuario y la contraseña se especifican en la conexion.
             */

            if (Seleccion == null)
                throw new ArgumentNullException("Seleccion");
            
            if (Usuario == null)
                throw new ArgumentNullException("Usuario");
            
            if (Contrasena == null)
                throw new ArgumentNullException("Contrasena");

            SecureString RutaDeConexion = new SecureString();
            
            // 1) Servidor
            RutaDeConexion.AgregarString("Server=");
            
            if((Seleccion.MetodoDeConexion == Constantes.MetodosDeConexion.TCP_IP) ||
                (Seleccion.MetodoDeConexion == Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA) ||
                (Seleccion.MetodoDeConexion == Constantes.MetodosDeConexion.VIA))
            {
                RutaDeConexion.AgregarString(RutaServidorFormatoTCPIP(Seleccion) + ";");
            }
            else if (Seleccion.MetodoDeConexion == Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE)
            {
                RutaDeConexion.AgregarString(RutaServidorFormatoCanalizaciones(Seleccion) + ";");
            }

            // 2) Indicamos que vamos a proporcionar el usuario y la contraseña de forma manual
            RutaDeConexion.AgregarString("Integrated Security=false;");

            // 3) Requerimos pooling
            RutaDeConexion.AgregarString("Pooling=true;");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            RutaDeConexion.AgregarString("Persist Security Info=false;");
            
            // 5) Nombre de usuario
            RutaDeConexion.AgregarString(("User ID=" + Usuario.ConvertirAUnsecureString()) + ";");

            // 6) Contraseña
            RutaDeConexion.AgregarString(("Password=" + Contrasena.ConvertirAUnsecureString()));

            /*
             * De la instruccion anterior (agregar password) hasta la siguiente (return) hay un hueco de 
             * seguridad porque cualquiera puede leer la contraseña al acceder a los miembros de RutaDeConexion
             */

            return RutaDeConexion;
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return _Conexion.State; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion 
        
        #region Funciones
        
        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena) 
        {
            if (_Conexion != null)
                _Conexion.Close();

            try
            {
                _Conexion.ConnectionString = CrearRutaDeAcceso(DatosDeConexion, Usuario, Contrasena).ConvertirAUnsecureString();
                _Conexion.Open();
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 18456:
                        throw new Exception("Usuario/clave inválido, intente nuevamente", ex);
                    case 18470:
                        throw new Exception("Cuenta deshabilitada", ex);
                    case 15007:
                        throw new Exception("Cuenta ya conectada", ex);
                    case 15151:
                        throw new Exception("Cuenta inexistente", ex);
                    default:
                        throw new Exception("Error en la conexión", ex);
                }
            }
        }

        public void Desconectar()
        {
            try
            {
                _Conexion.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            string[] ResultadoBruto = LectorSimple("SELECT name FROM sys.databases ORDER BY name");

            var ResultadoFinal = from R in ResultadoBruto
                                 where R != "master" && R != "tempdb" && R != "model" && R != "msdb"
                                 select R;

            //string[] ResultadoBruto = LectorSimple("EXEC sp_databases");
            /*
            List<string> ResultadoFinal = new List<string>();

            foreach (string ResultadoParcial in ResultadoBruto)
                ResultadoFinal.Add(ResultadoParcial);
            */
            return ResultadoFinal.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            CambiarBaseDeDatos(BaseDeDatos);
            string[] Resultado = LectorSimple("SELECT name FROM " + BaseDeDatos + "..sysobjects WHERE xtype = 'U' ORDER BY name");

            return Resultado.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            CambiarBaseDeDatos(BaseDeDatos);

            // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
            //DataTable Descripcion = LectorAvanzado("EXEC sp_columns @table_name = " + Tabla);
            string[] Descripcion = LectorSimple("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Tabla + "' ORDER BY ORDINAL_POSITION");

            string Columnas = string.Join(", ", Descripcion.ToArray());

            /*
             * Ahora si seleccionamos solo las columnas visibles. Un SELECT * FROM podria 
             * generar un error si el usuario no tiene los privilegios suficientes
             */
            return LectorAvanzado("SELECT " + Columnas + " FROM " + Tabla);
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            object Resultado = null;
            string SQL = string.Empty;

            /* 
             * Por lo general, se procede a crear un "login" y luego un usuario asociado a ese "login".
             * De esta forma:
             * 
             * CREATE LOGIN AbolrousHazem WITH PASSWORD = '340$Uuxwp7Mcxo7Khy';
             * USE AdventureWorks2008R2;
             * CREATE USER AbolrousHazem FOR LOGIN AbolrousHazem;
             * GO 
             * 
             * CREATE LOGIN loginName { WITH <option_list1> | FROM <sources> }
             * 
             * <option_list1> ::= 
             *      PASSWORD = { 'password' | hashed_password HASHED } [ MUST_CHANGE ]
             *      [ , <option_list2> [ ,... ] ]
             *      
             * <option_list2> ::=  
             *      SID = sid
             *      | DEFAULT_DATABASE =database    
             *      | DEFAULT_LANGUAGE =language
             *      | CHECK_EXPIRATION = { ON | OFF}
             *      | CHECK_POLICY = { ON | OFF}
             *      | CREDENTIAL =credential_name <sources> ::=
             *      WINDOWS [ WITH <windows_options>[ ,... ] ]
             *      | CERTIFICATE certname
             *      | ASYMMETRIC KEY asym_key_name<windows_options> ::=      
             *      DEFAULT_DATABASE =database
             *      | DEFAULT_LANGUAGE =language
             * 
             * CREATE USER user_name 
             *      [   
             *          { { FOR | FROM }
             *          { 
             *              LOGIN login_name 
             *              | CERTIFICATE cert_name 
             *              | ASYMMETRIC KEY asym_key_name
             *          } 
             *          | WITHOUT LOGIN
             *      ] 
             *      [ WITH DEFAULT_SCHEMA = schema_name ]
             *
             * GRANT { ALL [ PRIVILEGES ] }
             *       | permission [ ( column [ ,...n ] ) ] [ ,...n ]
             *       [ ON [ class :: ] securable ] TO principal [ ,...n ] 
             *       [ WITH GRANT OPTION ] [ AS principal ]
             *       
             */

            // 1) Determinamos los privilegios otorgados al nuevo usuario
            List<string> PrivilegiosLista = new List<string>();

            for (int i = 0; i < OrdenesComunes.Privilegios.Count; i++)
            {
                if ((Privilegios & (1 << i)) == 1)
                {
                    PrivilegiosLista.Add(OrdenesComunes.Privilegios[(1 << i)]);
                }
            }

            // 2) Identificamos las columnas a las cuales se aplican estos privilegios
            Dictionary<string, string> ColumnasDiccionario = new Dictionary<string, string>();

            foreach (string S in Columnas)
            {
                string[] Columna = S.Split('\\');

                string BD_Tabla = Columna[1] + "." + Columna[2];

                if (ColumnasDiccionario.ContainsKey(BD_Tabla))
                {
                    ColumnasDiccionario[BD_Tabla] += ", " + Columna[3];
                }
                else
                {
                    ColumnasDiccionario.Add(BD_Tabla, Columna[3]);
                }
            }

            List<KeyValuePair<string, string>> ColumnasLista = ColumnasDiccionario.ToList();

            try
            {
                // 3) Chequeamos a ver si ya existe el Login en el sistema y, si es asi, lo eliminamos
                SQL = "IF EXISTS (SELECT name FROM sys.server_principals WHERE name = '" + Usuario.ConvertirAUnsecureString() + "')"
                      + " DROP LOGIN " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                
                // 4) Creamos el login (usuario + constraseña)
                SQL = "CREATE LOGIN " + Usuario.ConvertirAUnsecureString() + " WITH PASSWORD = '" 
                    + Contrasena.ConvertirAUnsecureString() + "'";
                EjecutarOrden(SQL);
                /*
                CambiarBaseDeDatos("master");

                // 5) Chequeamos a ver si ya existe el usuario en "master" y, si es asi, lo eliminamos
                SQL = "IF EXISTS (SELECT * FROM sys.database_principals WHERE name = '" + Usuario.ConvertirAUnsecureString() + "')"
                      + " DROP USER " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);

                // 6) Creamos un usuario nuevo en "master" y lo asociamos al login recien creado
                SQL = "CREATE USER " + Usuario.ConvertirAUnsecureString() + " FOR LOGIN "
                    + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                */
                int i = 0;
                foreach (KeyValuePair<string, string> Par in ColumnasLista)
                {
                    string[] BD_Tabla = Par.Key.Split('.');

                    CambiarBaseDeDatos(BD_Tabla[0]);

                    // 5) Chequeamos a ver si ya existe el usuario en la base de datos y, si es asi, lo eliminamos
                    SQL = "IF EXISTS (SELECT * FROM sys.database_principals WHERE name = '" + Usuario.ConvertirAUnsecureString() + "')"
                          + " DROP USER " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);

                    // 6) Creamos un usuario nuevo en la base de datos seleccionada y lo asociamos al login recien creado
                    SQL = "CREATE USER " + Usuario.ConvertirAUnsecureString() + " FOR LOGIN "
                        + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);

                    // 9) Otorgamos los privilegios de tablas/columnas para cada base de datos
                    SQL = "GRANT ";
                    for (int j = 0; j < PrivilegiosLista.Count; j++)
                    {
                        SQL += PrivilegiosLista[j] + " (" + Par.Value + ")";
                        if ((j + 1) < PrivilegiosLista.Count)
                        {
                            SQL += ", ";
                        }
                    }

                    SQL += " ON OBJECT::dbo." + BD_Tabla[1] + " TO " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);
                    
                    SQL = "GRANT VIEW DEFINITION TO " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);

                    /*
                    // Este paso es necesario para que pueda ejecutar sp_tables y sp_columns
                    SQL = "GRANT SELECT ON SCHEMA::dbo TO " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);
                                        
                    // Creamos un "wrapper" de sp_databases
                    SQL = "IF EXISTS (SELECT * FROM sys.objects WHERE name = 'sp_databases' AND type = 'P')"
                          + "DROP PROCEDURE dbo.sp_databases";
                    EjecutarOrden(SQL);
                    
                    SQL = "CREATE PROCEDURE dbo.sp_databases AS"
                          + " EXEC sys.sp_databases";
                    EjecutarOrden(SQL);

                    CambiarBaseDeDatos("master");

                    // Le damos privilegios al usuario para que ejecute este procedimiento
                    SQL = "GRANT EXECUTE ON OBJECT::dbo.sp_databases TO " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);
                                        
                    SQL = "CREATE CERTIFICATE Certificado" + i.ToString() 
                          + " ENCRYPTION BY PASSWORD = 'All you need is love'"
                          + " WITH SUBJECT = 'Certificado para dbo.sp_databases',"
                          + " START_DATE = '20110101', EXPIRY_DATE = '20210101'";
                    EjecutarOrden(SQL);

                    SQL = "CREATE USER UsuarioCertificado" + i.ToString() 
                          + " FROM CERTIFICATE Certificado" + i.ToString();
                    EjecutarOrden(SQL);

                    SQL = "GRANT EXECUTE ON sys.sp_databases TO UsuarioCertificado" + i.ToString();
                    EjecutarOrden(SQL);

                    SQL = "ADD SIGNATURE TO OBJECT::dbo.sp_databases BY CERTIFICATE Certificado" + i.ToString()
                          + " WITH PASSWORD = 'All you need is love'";
                    EjecutarOrden(SQL);     
                    */
                    i++;
                }

                /*
                // 10) Otorgamos privilegios para que pueda explorar el servidor y las bases de datos
                CambiarBaseDeDatos("master");
                SQL = "GRANT VIEW ANY DEFINITION TO " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                */
                /*
                SQL = "GRANT EXECUTE ON sp_databases TO " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                SQL = "GRANT EXECUTE ON sp_tables TO " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                SQL = "GRANT EXECUTE ON sp_columns TO " + Usuario.ConvertirAUnsecureString();
                EjecutarOrden(SQL);
                 * */
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            throw new NotImplementedException();
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            throw new NotImplementedException();
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            throw new NotImplementedException();
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #endregion

        #region Tipos anidados

        public static class OrdenesComunes
        {
            public static Dictionary<int, string> Privilegios = new Dictionary<int, string>() 
            {
                { Constantes.Privilegios.NO_VALIDO, string.Empty },
                { Constantes.Privilegios.SELECCIONAR, "SELECT" },
                { Constantes.Privilegios.INSERTAR_FILAS, "INSERT" },
                { Constantes.Privilegios.ACTUALIZAR, "UPDATE" },
                { Constantes.Privilegios.BORRAR_FILAS, "DELETE" },
                { Constantes.Privilegios.INDIZAR, "INDEX" },
                { Constantes.Privilegios.ALTERAR, "ALTER" },
                { Constantes.Privilegios.CREAR, "CREATE" },
                { Constantes.Privilegios.DESTRUIR, "DROP" }
            };
        }

        #endregion
    }
}
