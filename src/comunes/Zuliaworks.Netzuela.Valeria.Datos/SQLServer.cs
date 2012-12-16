namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                  // DataTable
    using System.Data.SqlClient;                        // SqlConnection
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal, ParametrosDeConexion
    
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos SQLServer
    /// </summary>
    public partial class SQLServer : ConectorGenerico<SqlConnection, SqlCommand, SqlDataAdapter>
    {
        #region Variables y Constantes

        private static readonly Dictionary<int, string> PrivilegiosAOrdenes = new Dictionary<int, string>() 
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

        public SQLServer(ParametrosDeConexion servidorBd)
            : base(servidorBd)
        {
        }

        ~SQLServer()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        protected override string DescribirTabla(string tabla)
        {
            string[] descripcion = this.LectorSimple("SELECT subentity_name FROM fn_my_permissions('dbo." + tabla + "', 'Object') WHERE permission_name = 'SELECT' AND datalength(subentity_name) > 0");
            return string.Join(", ", descripcion.ToArray());
        }

        private string RutaServidorFormatoTcpIp(ParametrosDeConexion seleccion)
        {
            List<string> rutaDeConexion = new List<string>();
            
            switch (seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    rutaDeConexion.Add("tcp:");
                    break;
                case MetodosDeConexion.MemoriaCompartida:
                    rutaDeConexion.Add("lpc:");
                    break;
                case MetodosDeConexion.Via:
                    rutaDeConexion.Add("via:");
                    break;
                default:
                    throw new Exception("No se reconoce el metodo de conexion: \"" + seleccion.MetodoDeConexion + "\"");
            }

            rutaDeConexion.Add((seleccion.Anfitrion == "localhost") ? "." : seleccion.Anfitrion);
            rutaDeConexion.Add("\\" + seleccion.Instancia);

            if (seleccion.ArgumentoDeConexion != null && seleccion.ArgumentoDeConexion != string.Empty && seleccion.ArgumentoDeConexion != "Por defecto")
            {
                rutaDeConexion.Add("," + seleccion.ArgumentoDeConexion);
            }

            return string.Join(string.Empty, rutaDeConexion.ToArray());
        }

        private string RutaServidorFormatoCanalizaciones(ParametrosDeConexion seleccion)
        {
            if (seleccion.MetodoDeConexion != MetodosDeConexion.CanalizacionesConNombre)
            {
                throw new Exception("No se reconoce el metodo de conexion: \"" + seleccion.MetodoDeConexion + "\"");
            }

            return "np:" + seleccion.ArgumentoDeConexion;
        }

        protected override SecureString CrearRutaDeAcceso(ParametrosDeConexion seleccion, SecureString usuario, SecureString contrasena)
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

            if (seleccion == null)
            {
                throw new ArgumentNullException("Seleccion");
            }

            if (usuario == null)
            {
                throw new ArgumentNullException("Usuario");
            }

            if (contrasena == null)
            {
                throw new ArgumentNullException("Contrasena");
            }

            SecureString rutaDeConexion = new SecureString();
            
            // 1) Servidor
            rutaDeConexion.AgregarString("Server=");
            
            if ((seleccion.MetodoDeConexion == MetodosDeConexion.TcpIp) ||
                (seleccion.MetodoDeConexion == MetodosDeConexion.MemoriaCompartida) ||
                (seleccion.MetodoDeConexion == MetodosDeConexion.Via))
            {
                rutaDeConexion.AgregarString(this.RutaServidorFormatoTcpIp(seleccion) + ";");
            }
            else if (seleccion.MetodoDeConexion == MetodosDeConexion.CanalizacionesConNombre)
            {
                rutaDeConexion.AgregarString(this.RutaServidorFormatoCanalizaciones(seleccion) + ";");
            }

            // 2) Indicamos que vamos a proporcionar el usuario y la contraseña de forma manual
            rutaDeConexion.AgregarString("Integrated Security=false;");

            // 3) Requerimos pooling
            rutaDeConexion.AgregarString("Pooling=false;");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            rutaDeConexion.AgregarString("Persist Security Info=false;");
            
            // 5) Nombre de usuario
            rutaDeConexion.AgregarString("User ID=" + usuario.ConvertirAUnsecureString() + ";");

            // 6) Contraseña
            rutaDeConexion.AgregarString("Password=" + contrasena.ConvertirAUnsecureString());

            /*
             * De la instruccion anterior (agregar password) hasta la siguiente (return) hay un hueco de 
             * seguridad porque cualquiera puede leer la contraseña al acceder a los miembros de RutaDeConexion
             */

            return rutaDeConexion;
        }

        #endregion

        #region Implementaciones de interfaces
        
        #region Funciones
        
        #region Métodos sincrónicos

        public override void Conectar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                this.Desconectar();
                this.conexion.ConnectionString = this.CrearRutaDeAcceso(this.DatosDeConexion, usuario, contrasena).ConvertirAUnsecureString();
                this.conexion.Open();
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

        public override void Desconectar()
        {
            try
            {
                if (this.conexion != null)
                {
                    this.conexion.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos", ex);
            }
        }

        public override string[] ListarBasesDeDatos()
        {
            List<string> resultadoFinal = null;

            try
            {
                //string[] ResultadoBruto = LectorSimple("EXEC sp_databases");
                string[] resultadoBruto = this.LectorSimple("SELECT name FROM sys.databases ORDER BY name");
                resultadoFinal = new List<string>();

                foreach (string R in resultadoBruto)
                {
                    if (R != "master" && R != "tempdb" && R != "model" && R != "msdb")
                        resultadoFinal.Add(R);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al listar las bases de datos. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultadoFinal.ToArray();
        }

        public override string[] ListarTablas(string baseDeDatos)
        {
            List<string> resultado = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);
                //string[] ResultadoBruto = LectorSimple("EXEC sp_tables");
                string[] resultadoBruto = this.LectorSimple("SELECT name FROM " + baseDeDatos + "..sysobjects WHERE xtype = 'U' OR xtype = 'V' ORDER BY name");
                resultado = new List<string>();

                foreach (string s in resultadoBruto)
                {
                    resultado.Add(s);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al listar las tablas. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultado.ToArray();
        }

        public override DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            DataTable tablaLeida = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                //DataTable Descripcion = LectorAvanzado("EXEC sp_columns @table_name = " + Tabla);
                string columnas = this.DescribirTabla(tabla);

                /*
                 * Ahora si seleccionamos solo las columnas visibles. Un SELECT * FROM podria 
                 * generar un error si el usuario no tiene los privilegios suficientes
                 */
                tablaLeida = this.LectorAvanzado("SELECT " + columnas + " FROM " + tabla);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al leer la tabla. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return tablaLeida;
        }

        public override bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            bool resultado = false;

            try
            {
                /*
                 * Es necesario hacer toda esta parafernalia porque MySQL no puede editar una vista (VIEW) 
                 * directamente. 
                 * 
                 * Mas informacion sobre la "updatability" de una vista (VIEW): 
                 * http://dev.mysql.com/doc/refman/5.1/en/view-updatability.html
                 */

                /*
                 * Para pasar una cantidad variable de parametros a un procedimiento en SQL Server:
                 * http://www.sommarskog.se/dyn-search-2008.html
                 * http://vyaskn.tripod.com/passing_arrays_to_stored_procedures.htm
                 * http://www.sommarskog.se/share_data.html#tableparam <-- LEER ESTE
                 * http://lennilobel.wordpress.com/2009/07/29/sql-server-2008-table-valued-parameters-and-c-custom-iterators-a-match-made-in-heaven/
                 */

                DataSet settmp = new DataSet();
                this.CambiarBaseDeDatos(baseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                string columnas = this.DescribirTabla(nombreTabla);

                SqlDataAdapter adaptador = new SqlDataAdapter("SELECT " + columnas + " FROM " + nombreTabla, this.conexion);
                SqlCommandBuilder creadorDeOrden = new SqlCommandBuilder(adaptador);

                adaptador.FillSchema(settmp, SchemaType.Source);
                adaptador.Fill(settmp);
                settmp.Tables[0].TableName = nombreTabla;

                adaptador.InsertCommand = new SqlCommand("Insertar", this.conexion);
                adaptador.InsertCommand.CommandType = CommandType.StoredProcedure;
                adaptador.UpdateCommand = new SqlCommand("Actualizar", this.conexion);
                adaptador.UpdateCommand.CommandType = CommandType.StoredProcedure;
                adaptador.DeleteCommand = new SqlCommand("Eliminar", this.conexion);
                adaptador.DeleteCommand.CommandType = CommandType.StoredProcedure;

                SqlParameter variableDeEntradaSql = new SqlParameter("a_Parametros", string.Empty);
                variableDeEntradaSql.Direction = ParameterDirection.Input;

                adaptador.InsertCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.UpdateCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.DeleteCommand.Parameters.Add(variableDeEntradaSql);

                settmp.Merge(tabla, false, MissingSchemaAction.Error);

                SqlRowUpdatingEventHandler actualizandoFila = (r, a) =>
                {
                    List<string> parametros = new List<string>();
                    DataRowVersion version = DataRowVersion.Current;

                    if (a.StatementType == StatementType.Delete)
                    {
                        version = DataRowVersion.Original;
                    }

                    for (int i = 0; i < a.Row.Table.Columns.Count; i++)
                    {
                        parametros.Add(a.Row[i, version].ToString().Replace(",", "."));
                    }

                    a.Command.Parameters["a_Parametros"].Value = string.Join(",", parametros.ToArray());
                };

                SqlRowUpdatedEventHandler filaActualizada = (r, a) =>
                {
                    if (a.Errors != null)
                    {
                        if (a.Errors != null)
                        {
                            switch (a.Status)
                            {
                                case UpdateStatus.Continue:
                                    break;
                                case UpdateStatus.ErrorsOccurred:
                                    string msj = "Tipo de error=" + a.Errors.GetType().ToString()
                                            + "\nFilas afectadas=" + a.RecordsAffected.ToString()
                                            + "\nFila tiene errores=" + a.Row.HasErrors.ToString()
                                            + "\nTipo de instruccion ejecutada=" + a.StatementType
                                            + "\nEstatus de la fila=" + a.Status.ToString()
                                            + "\nErrores=" + a.Row.RowError;

                                    for (int i = 0; i < a.Row.ItemArray.Length; i++)
                                    {
                                        msj += "\nValorActual[" + i.ToString() + "]=" + a.Row[i, DataRowVersion.Current];
                                    }

                                    foreach (System.Collections.DictionaryEntry de in a.Errors.Data)
                                    {
                                        msj += "\nClave=" + de.Key.ToString() + "Valor=" + de.Value.ToString();
                                    }

                                    throw new Exception(msj, a.Errors);
                                case UpdateStatus.SkipAllRemainingRows:
                                    break;
                                case UpdateStatus.SkipCurrentRow:
                                    break;
                                default:
                                    throw new Exception("Estatus de fila desconocido", a.Errors);
                            }
                        }
                    }
                };

                adaptador.RowUpdating -= actualizandoFila;
                adaptador.RowUpdating += actualizandoFila;

                adaptador.RowUpdated -= filaActualizada;
                adaptador.RowUpdated += filaActualizada;

                // Primero actualizamos los borrados
                adaptador.Update(tabla.Select(null, null, DataViewRowState.Deleted));
                // Luego los modificados
                adaptador.Update(tabla.Select(null, null, DataViewRowState.ModifiedCurrent));
                // Y por ultimo los agregados
                adaptador.Update(tabla.Select(null, null, DataViewRowState.Added));

                resultado = true;
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo escribir la tabla. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        public override bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            bool resultado = false;
            string sql = string.Empty;

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
            List<string> privilegiosLista = new List<string>();

            for (int i = 0; i < SQLServer.PrivilegiosAOrdenes.Count; i++)
            {
                if ((privilegios & (1 << i)) == 1)
                {
                    privilegiosLista.Add(SQLServer.PrivilegiosAOrdenes[(1 << i)]);
                }
            }

            // 2) Identificamos las columnas a las cuales se aplican estos privilegios
            Dictionary<string, string> columnasDiccionario = new Dictionary<string, string>();

            foreach (string s in columnas)
            {
                string[] columna = s.Split('\\');

                string tabla = columna[1] + "." + columna[2];

                if (columnasDiccionario.ContainsKey(tabla))
                {
                    columnasDiccionario[tabla] += ", " + columna[3];
                }
                else
                {
                    columnasDiccionario.Add(tabla, columna[3]);
                }
            }

            List<KeyValuePair<string, string>> columnasLista = columnasDiccionario.ToList();

            try
            {
                // 3) Chequeamos a ver si ya existe el Login en el sistema y, si es asi, lo eliminamos
                sql = "IF EXISTS (SELECT name FROM sys.server_principals WHERE name = '" + usuario.ConvertirAUnsecureString() + "')"
                      + " DROP LOGIN " + usuario.ConvertirAUnsecureString();
                this.EjecutarOrden(sql);
                
                // 4) Creamos el login (usuario + constraseña)
                sql = "CREATE LOGIN " + usuario.ConvertirAUnsecureString() + " WITH PASSWORD = '" 
                    + contrasena.ConvertirAUnsecureString() + "'";
                this.EjecutarOrden(sql);

                foreach (KeyValuePair<string, string> par in columnasLista)
                {
                    string[] tabla = par.Key.Split('.');

                    this.CambiarBaseDeDatos(tabla[0]);

                    // 5) Chequeamos a ver si ya existe el usuario en la base de datos y, si es asi, lo eliminamos
                    sql = "IF EXISTS (SELECT * FROM sys.database_principals WHERE name = '" + usuario.ConvertirAUnsecureString() + "')"
                          + " DROP USER " + usuario.ConvertirAUnsecureString();
                    this.EjecutarOrden(sql);

                    // 6) Creamos un usuario nuevo en la base de datos seleccionada y lo asociamos al login recien creado
                    sql = "CREATE USER " + usuario.ConvertirAUnsecureString() + " FOR LOGIN "
                        + usuario.ConvertirAUnsecureString();
                    this.EjecutarOrden(sql);

                    // 9) Otorgamos los privilegios de tablas/columnas para cada base de datos
                    sql = "GRANT ";
                    for (int i = 0; i < privilegiosLista.Count; i++)
                    {
                        sql += privilegiosLista[i] + " (" + par.Value + ")";
                        if ((i + 1) < privilegiosLista.Count)
                        {
                            sql += ", ";
                        }
                    }

                    sql += " ON OBJECT::dbo." + tabla[1] + " TO " + usuario.ConvertirAUnsecureString();
                    this.EjecutarOrden(sql);

                    resultado = true;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        public override DataTable Consultar(string baseDeDatos, string sql)
        {
            DataTable resultado = null;
            this.CambiarBaseDeDatos(baseDeDatos);

            try
            {
                resultado = this.LectorAvanzado(sql);
            }
            catch (SqlException ex)
            {
                throw new Exception("No se pudo realizar la consulta. Error MSSQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        #endregion

        #endregion

        #endregion
    }
}
