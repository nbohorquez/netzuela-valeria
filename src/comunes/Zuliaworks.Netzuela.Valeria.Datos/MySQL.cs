namespace Zuliaworks.Netzuela.Valeria.Datos {
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
    public partial class MySQL : ConectorGenerico<MySqlConnection, MySqlCommand, MySqlDataAdapter> {
        #region Variables y Constantes

        private static readonly Dictionary<int, string> PrivilegiosAOrdenes = new Dictionary<int, string>()  {
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

        public MySQL(ParametrosDeConexion servidorBd) 
            : base(servidorBd)  {
        }

        ~MySQL() {
            this.Dispose(false);
        }

        #endregion

        #region Funciones
        
        protected override string DescribirTabla(string tabla) {
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

        protected override SecureString CrearRutaDeAcceso(ParametrosDeConexion seleccion, SecureString usuario, SecureString contrasena) {
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

            if (seleccion == null) {
                throw new ArgumentNullException("seleccion");
            } if (usuario == null) {
                throw new ArgumentNullException("usuario");
            } if (contrasena == null) {
                throw new ArgumentNullException("contrasena");
            }

            SecureString rutaDeConexion = new SecureString();

            // 1) Nombre del servidor anfitrion
            rutaDeConexion.AgregarString("Host=" + seleccion.Anfitrion + ";");

            // 2) Metodo de conexion
            switch (seleccion.MetodoDeConexion) {
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

        #region Funciones

        #region Métodos sincrónicos

        public override void Conectar(SecureString usuario, SecureString contrasena) {
            try {
                this.Desconectar();
                this.conexion.ConnectionString = this.CrearRutaDeAcceso(this.DatosDeConexion, usuario, contrasena).ConvertirAUnsecureString();
                this.conexion.Open();
            } catch (MySqlException ex) {
                switch (ex.Number) {
                    case 0:
                        throw new Exception("No se puede conectar al servidor. Contacte al administrador", ex);
                    case 1045:
                        throw new Exception("Usuario/clave inválido, intente nuevamente", ex);
                    default:
                        throw new Exception("Error en la conexión", ex);
                }
            }
        }

        public override void Desconectar() {
            try {
                if (this.conexion != null) {
                    this.conexion.Close();
                }
            } catch (MySqlException ex) {
                throw new Exception("Error al cerrar la conexión con la base de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }
        }

        public override string[] ListarBasesDeDatos() {
            List<string> resultadoFinal = null;

            try {
                string[] resultadoBruto = this.LectorSimple("SHOW DATABASES");
                resultadoFinal = new List<string>();

                foreach (string r in resultadoBruto) {
                    if (r != "information_schema" && r != "mysql" && r != "performance_schema") {
                        resultadoFinal.Add(r);
                    }
                }
            } catch (MySqlException ex) {
                throw new Exception("Error al listar las bases de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultadoFinal.ToArray();
        }

        public override string[] ListarTablas(string baseDeDatos) {
            List<string> resultado = null;

            try {
                this.CambiarBaseDeDatos(baseDeDatos);
                string[] resultadoBruto = this.LectorSimple("SHOW TABLES");
                resultado = new List<string>();

                foreach (string s in resultadoBruto) {
                    resultado.Add(s);
                }
            } catch (MySqlException ex) {
                throw new Exception("Error al listar las tablas. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultado.ToArray();
        }

        public override DataTable LeerTabla(string baseDeDatos, string tabla) {
            DataTable tablaLeida = null;

            try {
                this.CambiarBaseDeDatos(baseDeDatos);
                /*
                 * Tenemos que ver primero cuales son las columnas a las que tenemos acceso. Un "SELECT * 
                 * FROM ..." podria generar un error si el usuario no tiene los privilegios suficientes.
                 */
                string columnas = this.DescribirTabla(tabla);
                tablaLeida = this.LectorAvanzado("SELECT " + columnas + " FROM " + tabla);
            } catch (MySqlException ex) {
                throw new Exception("Error al leer la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return tablaLeida;
        }

        public override bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla) {
            throw new NotImplementedException();
        }

        public override bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios) {
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

            for (int i = 0; i < MySQL.PrivilegiosAOrdenes.Count; i++) {
                if ((privilegios & (1 << i)) == 1) {
                    privilegiosLista.Add(MySQL.PrivilegiosAOrdenes[(1 << i)]);
                }
            }

            // 2) Identificamos las columnas a las cuales se aplican estos privilegios
            Dictionary<string, string> columnasDiccionario = new Dictionary<string, string>();

            foreach (string s in columnas) {
                string[] columna = s.Split('\\');
                string tabla = columna[1] + "." + columna[2];

                if (columnasDiccionario.ContainsKey(tabla)) {
                    columnasDiccionario[tabla] += ", " + columna[3];
                } else {
                    columnasDiccionario.Add(tabla, columna[3]);
                }
            }

            List<KeyValuePair<string, string>> columnasLista = columnasDiccionario.ToList();

            try {
                // 3) Chequeamos a ver si ya existe el usuario en el sistema
                sql = "SELECT user FROM mysql.user WHERE user = '" + usuario.ConvertirAUnsecureString() + "' AND host = 'localhost'";
                List<string> usuariosExistentes = this.LectorSimple(sql).ToList();
                
                // 4) Si es asi, se elimina
                if (usuariosExistentes.Contains(usuario.ConvertirAUnsecureString())) {
                    sql = "DROP USER '" + usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    this.EjecutarOrden(sql);
                }
                
                // 5) Creamos el usuario con su contraseña
                sql = "CREATE USER '" + usuario.ConvertirAUnsecureString() + "'@'localhost' " +
                    "IDENTIFIED BY '" + contrasena.ConvertirAUnsecureString() + "'";
                this.EjecutarOrden(sql);

                // 6) Otorgamos los privilegios de columnas
                foreach (KeyValuePair<string, string> par in columnasLista) {
                    sql = "GRANT ";

                    for (int i = 0; i < privilegiosLista.Count; i++) {
                        sql += privilegiosLista[i] + " (" + par.Value + ")";
                        if ((i + 1) < privilegiosLista.Count) {
                            sql += ", ";
                        }
                    }

                    sql += " ON " + par.Key + " TO '" + usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    this.EjecutarOrden(sql);
                }

                // 7) Actualizamos la cache de privilegios del servidor
                this.EjecutarOrden("FLUSH PRIVILEGES");
                resultado = true;
            } catch (MySqlException ex) {
                throw new Exception("No se pudo crear el usuario especificado. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        public override DataTable Consultar(string baseDeDatos, string sql) {
            DataTable resultado = null;
            this.CambiarBaseDeDatos(baseDeDatos);

            try {
                resultado = this.LectorAvanzado(sql);
            } catch (MySqlException ex) {
                throw new Exception("No se pudo realizar la consulta. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return resultado;
        }

        #endregion

        #endregion

        #endregion
    }
}
