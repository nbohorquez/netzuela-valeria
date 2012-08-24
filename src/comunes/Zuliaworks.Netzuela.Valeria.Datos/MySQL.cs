namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                              // ConnectionState, DataTable
    using System.Linq;
    using System.Security;                          // SecureString
    using System.Text;

    using MySql.Data.MySqlClient;                   // MySqlConnection
    using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos MySQL
    /// </summary>
    public partial class MySQL : EventosComunes, IBaseDeDatosLocal
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

        private MySqlConnection conexion;
        
        #endregion

        #region Constructores

        public MySQL(ParametrosDeConexion servidorBd)
        {
            this.DatosDeConexion = servidorBd;
            this.conexion = new MySqlConnection();
            this.conexion.StateChange -= this.ManejarCambioDeEstado;
            this.conexion.StateChange += this.ManejarCambioDeEstado;
        }

        ~MySQL()
        {
            this.Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;

            if (borrarCodigoAdministrado)
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
            
            MySqlCommand orden = new MySqlCommand(sql, this.conexion);
            orden.ExecuteNonQuery();
        }

        private string[] LectorSimple(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            MySqlDataReader lector = null;
            List<string> resultado = new List<string>();
            MySqlCommand orden = new MySqlCommand(sql, this.conexion);
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

            MySqlDataAdapter adaptador = new MySqlDataAdapter(sql, this.conexion);
            MySqlCommandBuilder creadorDeOrden = new MySqlCommandBuilder(adaptador);

            adaptador.FillSchema(resultado, SchemaType.Source);
            adaptador.Fill(resultado);

            return resultado;
        }

        private string DescribirTabla(string tabla)
        {
            DataTable descripcion = this.LectorAvanzado("DESCRIBE " + tabla);

            var columnasPermitidas = from D in descripcion.AsEnumerable()
                                     select D.Field<string>("Field");

            return string.Join(", ", columnasPermitidas.ToArray());
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
             * La lista completa de las opciones de la ruta de conexion ("Connection String" en ingles) se detalla en:
             * http://dev.mysql.com/doc/refman/5.5/en/connector-net-connection-options.html
             * 
             * SERVIDOR
             * ========
             * "Host=", "Server=", "Data Source=", "DataSource=", "Address=", "Addr=", "Network Address="
             * ---------------------
             * "Server=(localhost);"
             * 
             * Nombre del anfitrion o la direccion ip. Por defecto es "localhost"
             * 
             * BASE DE DATOS
             * =============
             * "Initial Catalog=", "Database="
             * --------------------------------
             * "Database=(mysql);"
             * 
             * Nombre de la base de datos inicial a utilizar. Por defecto es "mysql"
             * 
             * PROTOCOLO DE CONEXION
             * =====================
             * "Protocol=(socket)"; 
             * 
             * Protocolo que se va a emplear para comunicarse con el servidor. Por defecto es "socket".
             * Las opciones disponibles son:
             * 
             *      *- "socket", "tcp": TCP/IP
             *      *- "pipe": canalizaciones con nombre
             *      *- "unix": conexion por sockets de unix
             *      *- "memory": memoria compartida
             * 
             * TCP/IP
             * ======
             * "Port="
             * -------
             * "Port=3306"
             * 
             * Puerto utilizado para la conexion via TCP/IP con el servidor. Por defecto es "3306".
             * 
             * MEMORIA COMPARTIDA
             * ==================
             * "Shared Memory Name=(MYSQL)"
             * 
             * Nombre de la seccion de memoria compartida que va a ser empleada para establecer comunicacion con 
             * el servidor. Por defecto es "MYSQL"
             * 
             * CANALIZACIONES CON NOMBRE
             * =========================
             * "Pipe Name=", "Pipe="
             * 
             * Nombre del archivo de canalizacion que va a ser empleado para establecer comunicacion con 
             * el servidor. Por defecto es "mysql"
             * 
             * USUARIO
             * =======
             * "User Id=", "Username=", "Uid=", "User name="
             * 
             * Nombre de usuario/cuenta empleado en el proceso de autentificacion. No tiene valor por defecto
             * 
             * CONTRASEÑA
             * ==========
             * "Password=", "pwd="
             * 
             * Contraseña asociada al usuario.
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

            // 1) Nombre del servidor anfitrion
            rutaDeConexion.AgregarString("Host=" + seleccion.Anfitrion + ";");

            // 2) Metodo de conexion
            switch (seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    rutaDeConexion.AgregarString("Protocol=\"tcp\";");
                    rutaDeConexion.AgregarString("Port=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                case MetodosDeConexion.CanalizacionesConNombre:
                    rutaDeConexion.AgregarString("Protocol=\"pipe\";");
                    rutaDeConexion.AgregarString("Pipe=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                case MetodosDeConexion.MemoriaCompartida:
                    rutaDeConexion.AgregarString("Protocol=\"memory\";");
                    rutaDeConexion.AgregarString("Shared Memory Name=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                default:
                    break;
            }

            // 3) Requerimos pooling
            rutaDeConexion.AgregarString("Pooling=false;");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            rutaDeConexion.AgregarString("Persist Security Info=false;");

            // 5) Nombre de usuario
            rutaDeConexion.AgregarString("Username=" + usuario.ConvertirAUnsecureString() + ";");

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
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        throw new Exception("No se puede conectar al servidor. Contacte al administrador", ex);
                    case 1045:
                        throw new Exception("Usuario/clave inválido, intente nuevamente", ex);
                    default:
                        throw new Exception("Error en la conexión", ex);
                }
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
            catch (MySqlException ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> resultadoFinal = null;

            try
            {
                string[] resultadoBruto = this.LectorSimple("SHOW DATABASES");
                resultadoFinal = new List<string>();

                foreach (string r in resultadoBruto)
                {
                    if (r != "information_schema" && r != "mysql" && r != "performance_schema")
                    {
                        resultadoFinal.Add(r);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las bases de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultadoFinal.ToArray();
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            List<string> resultado = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);
                string[] resultadoBruto = this.LectorSimple("SHOW TABLES");
                resultado = new List<string>();

                foreach (string s in resultadoBruto)
                {
                    resultado.Add(s);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las tablas. Error MySQL No. " + ex.Number.ToString(), ex);
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
            catch (MySqlException ex)
            {
                throw new Exception("Error al leer la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return tablaLeida;
        }

        public bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
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

                DataSet settmp = new DataSet();
                this.CambiarBaseDeDatos(baseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                string columnas = this.DescribirTabla(nombreTabla);

                MySqlDataAdapter adaptador = new MySqlDataAdapter("SELECT " + columnas + " FROM " + nombreTabla, this.conexion);
                //MySqlCommandBuilder creadorDeOrden = new MySqlCommandBuilder(adaptador);
                
                adaptador.FillSchema(settmp, SchemaType.Source);
                adaptador.Fill(settmp);
                settmp.Tables[0].TableName = nombreTabla;
                
                adaptador.InsertCommand = new MySqlCommand("Insertar", this.conexion);
                adaptador.InsertCommand.CommandType = CommandType.StoredProcedure;
                adaptador.UpdateCommand = new MySqlCommand("Actualizar", this.conexion);
                adaptador.UpdateCommand.CommandType = CommandType.StoredProcedure;
                adaptador.DeleteCommand = new MySqlCommand("Eliminar", this.conexion);
                adaptador.DeleteCommand.CommandType = CommandType.StoredProcedure;

                MySqlParameter variableDeEntradaSql = new MySqlParameter("a_Parametros", string.Empty);
                variableDeEntradaSql.Direction = ParameterDirection.Input;

                adaptador.InsertCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.UpdateCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.DeleteCommand.Parameters.Add(variableDeEntradaSql);

                // Este paso es crucial. Por alguna razon no se pueden reforzar los "constraints"
				// luego de hacer un "merge" con filas que han sido eliminadas (no hay problema 
				// con las filas modificadas o agregadas)
				// http://social.msdn.microsoft.com/Forums/en-US/Vsexpressvb/thread/27aec612-5ca4-41ba-80d6-0204893fdcd1/
				settmp.EnforceConstraints = false;
				// http://msdn.microsoft.com/es-es/library/wtk78t63%28v=vs.80%29.aspx
                settmp.Merge(tabla, false, MissingSchemaAction.Error);
                
                MySqlRowUpdatingEventHandler actualizandoFila = (r, a) =>
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
                
                MySqlRowUpdatedEventHandler filaActualizada = (r, a) =>
                {
                    // http://www.hesab.net/book/asp.net/Additional%20Documents/Resolving%20Conflicts%20on%20a%20Row-by-Row%20Basis.pdf
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
                };

                adaptador.RowUpdating -= actualizandoFila;
                adaptador.RowUpdating += actualizandoFila;

                adaptador.RowUpdated -= filaActualizada;
                adaptador.RowUpdated += filaActualizada;

                /* 
                 * http://msdn.microsoft.com/es-es/library/33y2221y%28v=vs.100%29.aspx 
                 * https://github.com/mono/mono/blob/mono-2-10/mcs/class/System.Data/System.Data.Common/DbDataAdapter.cs 
                 * Estas instrucciones generaban excepciones de valor nulo porque no tenian una conexion especificada
                 */
                // Primero actualizamos los borrados
                adaptador.Update(settmp.Tables[0].Select(null, null, DataViewRowState.Deleted));
                // Luego los modificados
                adaptador.Update(settmp.Tables[0].Select(null, null, DataViewRowState.ModifiedCurrent));
                // Y por ultimo los agregados
                adaptador.Update(settmp.Tables[0].Select(null, null, DataViewRowState.Added));

                resultado = true;
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo escribir la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            bool resultado = false;
            string sql = string.Empty;

            /*
             * La estructura de esta orden es:
             * 
             * GRANT
             *      priv_type [(column_list)]
             *      [, priv_type [(column_list)]] ...
             *      ON [object_type] priv_level
             *      TO user_specification [, user_specification] ...
             *      [REQUIRE {NONE | ssl_option [[AND] ssl_option] ...}]
             *      [WITH with_option ...]
             *      
             * 
             * GRANT PROXY ON user_specification
             *      TO user_specification [, user_specification] ...
             *      [WITH GRANT OPTION]
             * 
             * object_type:
             *      TABLE
             *    | FUNCTION
             *    | PROCEDURE
             * 
             * priv_level:
             *      *
             *    | *.*
             *    | db_name.*
             *    | db_name.tbl_name
             *    | tbl_name
             *    | db_name.routine_name
             * 
             * user_specification:
             *      user
             *      [
             *          IDENTIFIED BY [PASSWORD] 'password'
             *          | IDENTIFIED WITH auth_plugin [AS 'auth_string']
             *      ]
             *      
             * ssl_option:
             *     SSL
             *   | X509
             *   | CIPHER 'cipher'
             *   | ISSUER 'issuer'
             *   | SUBJECT 'subject'
             *   
             * with_option:
             *     GRANT OPTION
             *   | MAX_QUERIES_PER_HOUR count
             *   | MAX_UPDATES_PER_HOUR count
             *   | MAX_CONNECTIONS_PER_HOUR count
             *   | MAX_USER_CONNECTIONS count
             */

            // 1) Determinamos los privilegios otorgados al nuevo usuario
            List<string> privilegiosLista = new List<string>();

            for (int i = 0; i < MySQL.PrivilegiosAOrdenes.Count; i++)
            {
                if ((privilegios & (1 << i)) == 1)
                {
                    privilegiosLista.Add(MySQL.PrivilegiosAOrdenes[(1 << i)]);
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
                // 3) Chequeamos a ver si ya existe el usuario en el sistema
                sql = "SELECT user FROM mysql.user WHERE user = '" + usuario.ConvertirAUnsecureString() + "' AND host = 'localhost'";
                List<string> usuariosExistentes = this.LectorSimple(sql).ToList();
                
                // 4) Si es asi, se elimina
                if (usuariosExistentes.Contains(usuario.ConvertirAUnsecureString()))
                {
                    sql = "DROP USER '" + usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    this.EjecutarOrden(sql);
                }
                
                // 5) Creamos el usuario con su contraseña
                sql = "CREATE USER '" + usuario.ConvertirAUnsecureString() + "'@'localhost' " +
                    "IDENTIFIED BY '" + contrasena.ConvertirAUnsecureString() + "'";
                this.EjecutarOrden(sql);

                // 6) Otorgamos los privilegios de columnas
                foreach (KeyValuePair<string, string> par in columnasLista)
                {
                    sql = "GRANT ";

                    for (int i = 0; i < privilegiosLista.Count; i++)
                    {
                        sql += privilegiosLista[i] + " (" + par.Value + ")";
                        if ((i + 1) < privilegiosLista.Count)
                        {
                            sql += ", ";
                        }
                    }

                    sql += " ON " + par.Key + " TO '" + usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    this.EjecutarOrden(sql);
                }

                // 7) Actualizamos la cache de privilegios del servidor
                this.EjecutarOrden("FLUSH PRIVILEGES");
                resultado = true;
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            DataTable resultado = null;
            this.CambiarBaseDeDatos(baseDeDatos);

            try
            {
                resultado = this.LectorAvanzado(sql);
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo realizar la consulta. Error MySQL No. " + ex.Number.ToString(), ex);
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

        public void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            throw new NotImplementedException();
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
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
