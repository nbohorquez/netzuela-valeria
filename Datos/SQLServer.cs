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
        protected static Dictionary<int, string> PrivilegiosAOrdenes = new Dictionary<int, string>() 
        {
            { Privilegios.NoValido, string.Empty },
            { Privilegios.Seleccionar, "SELECT" },
            { Privilegios.InsertarFilas, "INSERT" },
            { Privilegios.Actualizar, "UPDATE" },
            { Privilegios.BorrarFilas, "DELETE" },
            { Privilegios.Indizar, "INDEX" },
            { Privilegios.Alterar, "ALTER" },
            { Privilegios.Crear, "CREATE" },
            { Privilegios.Destruir, "DROP" }
        };

        #endregion

        #region Constructores

        public SQLServer(ParametrosDeConexion ServidorBD)
        {
            DatosDeConexion = ServidorBD;

            _Conexion = new SqlConnection();

            // Registramos el manejador de eventos por defecto. Este sirve como repetidor del evento subyacente.
            _Conexion.StateChange -= base.ManejarCambioDeEstado;
            _Conexion.StateChange += base.ManejarCambioDeEstado;
        }

        ~SQLServer()
        {
            Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;

            if (BorrarCodigoAdministrado)
            {
                this._Conexion.Dispose();
            }
        }

        private void CambiarBaseDeDatos(string BaseDeDatos)
        {
            if (_Conexion.Database != BaseDeDatos)
                _Conexion.ChangeDatabase(BaseDeDatos);
        }

        private void EjecutarOrden(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            SqlCommand Orden = new SqlCommand(SQL, _Conexion);
            Orden.ExecuteNonQuery();
        }

        private string[] LectorSimple(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            SqlDataReader Lector = null;
            List<string> Resultado = new List<string>();

            SqlCommand Orden = new SqlCommand(SQL, _Conexion);

            Lector = Orden.ExecuteReader();
            while (Lector.Read())
            {
                Resultado.Add(Lector.GetString(0));
            }
            
            Lector.Close();

            return Resultado.ToArray();
        }

        private DataTable LectorAvanzado(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            DataTable Resultado = new DataTable();

            SqlDataAdapter Adaptador = new SqlDataAdapter(SQL, _Conexion);
            SqlCommandBuilder CreadorDeOrden = new SqlCommandBuilder(Adaptador);

            Adaptador.FillSchema(Resultado, SchemaType.Source);
            Adaptador.Fill(Resultado);

            return Resultado;
        }

        private string DescribirTabla(string Tabla)
        {
            string[] Descripcion = LectorSimple("SELECT subentity_name FROM fn_my_permissions('dbo." + Tabla + "', 'Object') WHERE permission_name = 'SELECT' AND datalength(subentity_name) > 0");

            return string.Join(", ", Descripcion.ToArray());
        }

        private string RutaServidorFormatoTCPIP(ParametrosDeConexion Seleccion)
        {
            List<string> RutaDeConexion = new List<string>();
            
            switch (Seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    RutaDeConexion.Add("tcp:");
                    break;
                case MetodosDeConexion.MemoriaCompartida:
                    RutaDeConexion.Add("lpc:");
                    break;
                case MetodosDeConexion.Via:
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
            if(Seleccion.MetodoDeConexion != MetodosDeConexion.CanalizacionesConNombre)
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
            
            if((Seleccion.MetodoDeConexion == MetodosDeConexion.TcpIp) ||
                (Seleccion.MetodoDeConexion == MetodosDeConexion.MemoriaCompartida) ||
                (Seleccion.MetodoDeConexion == MetodosDeConexion.Via))
            {
                RutaDeConexion.AgregarString(RutaServidorFormatoTCPIP(Seleccion) + ";");
            }
            else if (Seleccion.MetodoDeConexion == MetodosDeConexion.CanalizacionesConNombre)
            {
                RutaDeConexion.AgregarString(RutaServidorFormatoCanalizaciones(Seleccion) + ";");
            }

            // 2) Indicamos que vamos a proporcionar el usuario y la contraseña de forma manual
            RutaDeConexion.AgregarString("Integrated Security=false;");

            // 3) Requerimos pooling
            RutaDeConexion.AgregarString("Pooling=false;");

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
            try
            {
                Desconectar();

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
                if(_Conexion != null)
                    _Conexion.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> ResultadoFinal = null;

            try
            {
                //string[] ResultadoBruto = LectorSimple("EXEC sp_databases");
                string[] ResultadoBruto = LectorSimple("SELECT name FROM sys.databases ORDER BY name");

                ResultadoFinal = new List<string>();

                foreach (string R in ResultadoBruto)
                {
                    if(R != "master" && R != "tempdb" && R != "model" && R != "msdb")
                        ResultadoFinal.Add(R);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al listar las bases de datos. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return ResultadoFinal.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = null;

            try
            {
                CambiarBaseDeDatos(BaseDeDatos);
                //string[] ResultadoBruto = LectorSimple("EXEC sp_tables");
                string[] ResultadoBruto = LectorSimple("SELECT name FROM " + BaseDeDatos + "..sysobjects WHERE xtype = 'U' OR xtype = 'V' ORDER BY name");

                Resultado = new List<string>();

                foreach (string S in ResultadoBruto)
                    Resultado.Add(S);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al listar las tablas. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            DataTable TablaLeida = null;

            try
            {
                CambiarBaseDeDatos(BaseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                //DataTable Descripcion = LectorAvanzado("EXEC sp_columns @table_name = " + Tabla);
                string Columnas = DescribirTabla(Tabla);

                /*
                 * Ahora si seleccionamos solo las columnas visibles. Un SELECT * FROM podria 
                 * generar un error si el usuario no tiene los privilegios suficientes
                 */
                TablaLeida = LectorAvanzado("SELECT " + Columnas + " FROM " + Tabla);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al leer la tabla. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return TablaLeida;
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            bool Resultado = false;

            try
            {
                /*
                 * Es necesario hacer toda esta parafernalia porque MySQL no puede editar una vista (VIEW) 
                 * directamente. 
                 * 
                 * Mas informacion sobre la "updatability" de una vista (VIEW): 
                 * http://dev.mysql.com/doc/refman/5.1/en/view-updatability.html
                 */

                DataTable Temporal = new DataTable();

                CambiarBaseDeDatos(BaseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                string Columnas = DescribirTabla(NombreTabla);

                SqlDataAdapter Adaptador = new SqlDataAdapter("SELECT " + Columnas + " FROM " + NombreTabla, _Conexion);
                SqlCommandBuilder CreadorDeOrden = new SqlCommandBuilder(Adaptador);

                Adaptador.FillSchema(Temporal, SchemaType.Source);
                Adaptador.Fill(Temporal);

                Adaptador.InsertCommand = new SqlCommand("Insertar");
                Adaptador.InsertCommand.CommandType = CommandType.StoredProcedure;
                Adaptador.UpdateCommand = new SqlCommand("Actualizar");
                Adaptador.UpdateCommand.CommandType = CommandType.StoredProcedure;
                Adaptador.DeleteCommand = new SqlCommand("Eliminar");
                Adaptador.DeleteCommand.CommandType = CommandType.StoredProcedure;

                string VariableDeEntrada = string.Empty;

                SqlParameter VariableDeEntradaSQL = new SqlParameter("a_Parametros", VariableDeEntrada);
                VariableDeEntradaSQL.Direction = ParameterDirection.Input;

                Adaptador.InsertCommand.Parameters.Add(VariableDeEntradaSQL);
                Adaptador.UpdateCommand.Parameters.Add(VariableDeEntradaSQL);
                Adaptador.DeleteCommand.Parameters.Add(VariableDeEntradaSQL);

                //Temporal.Merge(Tabla, false, MissingSchemaAction.Error);

                SqlRowUpdatingEventHandler ActualizandoFila = (r, a) =>
                {
                    List<string> Parametros = new List<string>();

                    foreach (object Dato in a.Row.ItemArray)
                    {
                        Parametros.Add(Dato.ToString().Replace(",", "."));
                    }

                    VariableDeEntrada = string.Join(",", Parametros.ToArray());
                    a.Command.Parameters[0].Value = VariableDeEntrada;
                };

                SqlRowUpdatedEventHandler FilaActualizada = (r, a) =>
                {
                    if (a.Errors != null)
                    {
                        throw a.Errors;
                    }
                };

                Adaptador.RowUpdating -= ActualizandoFila;
                Adaptador.RowUpdating += ActualizandoFila;

                Adaptador.RowUpdated -= FilaActualizada;
                Adaptador.RowUpdated += FilaActualizada;

                // Primero actualizamos los borrados
                Adaptador.Update(Tabla.Select(null, null, DataViewRowState.Deleted));
                // Luego los modificados
                Adaptador.Update(Tabla.Select(null, null, DataViewRowState.ModifiedCurrent));
                // Y por ultimo los agregados
                Adaptador.Update(Tabla.Select(null, null, DataViewRowState.Added));

                Resultado = true;
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo escribir la tabla. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        public bool CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            bool Resultado = false;
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

            for (int i = 0; i < PrivilegiosAOrdenes.Count; i++)
            {
                if ((Privilegios & (1 << i)) == 1)
                {
                    PrivilegiosLista.Add(PrivilegiosAOrdenes[(1 << i)]);
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
                    for (int i = 0; i < PrivilegiosLista.Count; i++)
                    {
                        SQL += PrivilegiosLista[i] + " (" + Par.Value + ")";
                        if ((i + 1) < PrivilegiosLista.Count)
                        {
                            SQL += ", ";
                        }
                    }

                    SQL += " ON OBJECT::dbo." + BD_Tabla[1] + " TO " + Usuario.ConvertirAUnsecureString();
                    EjecutarOrden(SQL);

                    Resultado = true;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        public DataTable Consultar(string baseDeDatos, string Sql)
        {
            DataTable resultado = null;

            CambiarBaseDeDatos(baseDeDatos);

            try
            {
                resultado = LectorAvanzado(Sql);
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo realizar la consulta. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
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

        public void ConsultarAsinc(string baseDeDatos, string Sql)
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

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
