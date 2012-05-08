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
    public partial class MySQL : EventosComunes, IBaseDeDatos
    {
        #region Variables y constantes

        private static Dictionary<int, string> PrivilegiosAOrdenes = new Dictionary<int, string>() 
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

        public MySQL(ParametrosDeConexion ServidorBD)
        {
            this.DatosDeConexion = ServidorBD;

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

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;

            if (BorrarCodigoAdministrado)
            {
                this.conexion.Dispose();
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
            
            MySqlCommand Orden = new MySqlCommand(sql, this.conexion);
            Orden.ExecuteNonQuery();
        }

        private string[] LectorSimple(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("SQL");
            }

            MySqlDataReader Lector = null;
            List<string> Resultado = new List<string>();

            MySqlCommand Orden = new MySqlCommand(sql, this.conexion);

            Lector = Orden.ExecuteReader();
            while (Lector.Read())
            {
                Resultado.Add(Lector.GetString(0));
            }

            Lector.Close();

            return Resultado.ToArray();
        }

        private DataTable LectorAvanzado(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("SQL");
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

            SecureString RutaDeConexion = new SecureString();

            // 1) Nombre del servidor anfitrion
            RutaDeConexion.AgregarString("Host=" + seleccion.Anfitrion + ";");

            // 2) Metodo de conexion
            switch (seleccion.MetodoDeConexion)
            {
                case MetodosDeConexion.TcpIp:
                    RutaDeConexion.AgregarString("Protocol=\"tcp\";");
                    RutaDeConexion.AgregarString("Port=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                case MetodosDeConexion.CanalizacionesConNombre:
                    RutaDeConexion.AgregarString("Protocol=\"pipe\";");
                    RutaDeConexion.AgregarString("Pipe=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                case MetodosDeConexion.MemoriaCompartida:
                    RutaDeConexion.AgregarString("Protocol=\"memory\";");
                    RutaDeConexion.AgregarString("Shared Memory Name=" + seleccion.ArgumentoDeConexion + ";");
                    break;
                default:
                    break;
            }

            // 3) Requerimos pooling
            RutaDeConexion.AgregarString("Pooling=false;");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            RutaDeConexion.AgregarString("Persist Security Info=false;");

            // 5) Nombre de usuario
            RutaDeConexion.AgregarString(("Username=" + usuario.ConvertirAUnsecureString()) + ";");

            // 6) Contraseña
            RutaDeConexion.AgregarString(("Password=" + contrasena.ConvertirAUnsecureString()));

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
            get { return this.conexion.State; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion
        
        #region Funciones

        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            try
            {
                this.Desconectar();

                this.conexion.ConnectionString = this.CrearRutaDeAcceso(this.DatosDeConexion, Usuario, Contrasena).ConvertirAUnsecureString();
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
            List<string> ResultadoFinal = null;

            try
            {
                string[] ResultadoBruto = this.LectorSimple("SHOW DATABASES");
                ResultadoFinal = new List<string>();

                foreach (string R in ResultadoBruto)
                {
                    if (R != "information_schema" && R != "mysql" && R != "performance_schema")
                    {
                        ResultadoFinal.Add(R);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las bases de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return ResultadoFinal.ToArray();
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            List<string> Resultado = null;

            try
            {
                this.CambiarBaseDeDatos(baseDeDatos);
                string[] ResultadoBruto = this.LectorSimple("SHOW TABLES");
                Resultado = new List<string>();

                foreach (string S in ResultadoBruto)
                {
                    Resultado.Add(S);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las tablas. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado.ToArray();
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

                DataTable temporal = new DataTable();
                this.CambiarBaseDeDatos(baseDeDatos);

                // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
                string columnas = this.DescribirTabla(nombreTabla);

                MySqlDataAdapter adaptador = new MySqlDataAdapter("SELECT " + columnas + " FROM " + nombreTabla, this.conexion);
                MySqlCommandBuilder creadorDeOrden = new MySqlCommandBuilder(adaptador);
                
                adaptador.FillSchema(temporal, SchemaType.Source);
                adaptador.Fill(temporal);

                adaptador.InsertCommand = new MySqlCommand("Insertar");
                adaptador.InsertCommand.CommandType = CommandType.StoredProcedure;
                adaptador.UpdateCommand = new MySqlCommand("Actualizar");
                adaptador.UpdateCommand.CommandType = CommandType.StoredProcedure;
                adaptador.DeleteCommand = new MySqlCommand("Eliminar");
                adaptador.DeleteCommand.CommandType = CommandType.StoredProcedure;

                string variableDeEntrada = string.Empty;

                MySqlParameter variableDeEntradaSql = new MySqlParameter("a_Parametros", variableDeEntrada);
                variableDeEntradaSql.Direction = ParameterDirection.Input;

                adaptador.InsertCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.UpdateCommand.Parameters.Add(variableDeEntradaSql);
                adaptador.DeleteCommand.Parameters.Add(variableDeEntradaSql);

                //Temporal.Merge(Tabla, false, MissingSchemaAction.Error);
                
                MySqlRowUpdatingEventHandler actualizandoFila = (r, a) =>
                {
                    List<string> parametros = new List<string>();

                    foreach (object Dato in a.Row.ItemArray)
                    {
                        parametros.Add(Dato.ToString().Replace(",", "."));
                    }

                    variableDeEntrada = string.Join(",", parametros.ToArray());
                    a.Command.Parameters[0].Value = variableDeEntrada;
                };
                
                MySqlRowUpdatedEventHandler filaActualizada = (r, a) =>
                {
                    if (a.Errors != null)
                    {
                        throw a.Errors;
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

                Resultado = true;
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo escribir la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
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

            for (int i = 0; i < PrivilegiosAOrdenes.Count; i++)
            {
                if ((privilegios & (1 << i)) == 1)
                {
                    privilegiosLista.Add(PrivilegiosAOrdenes[(1 << i)]);
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

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
