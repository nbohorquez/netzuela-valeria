using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;                   // MySqlConnection
using System.Data;                              // ConnectionState, DataTable
using System.Security;                          // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos MySQL
    /// </summary>
    public partial class MySQL : IBaseDeDatos
    {
        #region Variables

        private MySqlConnection _Conexion;

        #endregion

        #region Constructores

        public MySQL(DatosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            _Conexion = new MySqlConnection();
        }

        #endregion

        #region Propiedades

        public DatosDeConexion Servidor { get; set; }

        #endregion

        #region Funciones

        private void CambiarBaseDeDatos(string BaseDeDatos)
        {
            try
            {
                if (_Conexion.Database != BaseDeDatos)
                    _Conexion.ChangeDatabase(BaseDeDatos);
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al cambiar la base de datos.\nError MySQL No. " + ex.Number.ToString(), ex);
            }
        }

        private void EjecutarOrden(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            try
            {
                MySqlCommand Orden = new MySqlCommand(SQL, _Conexion);
                Orden.ExecuteScalar();
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo ejecutar la orden.\nError MySQL No. " + ex.Number.ToString(), ex);
            }
        }

        private string[] LectorSimple(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            MySqlDataReader Lector = null;
            List<string> Resultado = new List<string>();

            try
            {
                MySqlCommand Orden = new MySqlCommand(SQL, _Conexion);

                Lector = Orden.ExecuteReader();
                while (Lector.Read())
                {
                    Resultado.Add(Lector.GetString(0));
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo obtener la lista de elementos desde la base de datos.\nError MySQL No. " + ex.Number.ToString(), ex);
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
                MySqlDataAdapter Adaptador = new MySqlDataAdapter(SQL, _Conexion);
                MySqlCommandBuilder CreadorDeOrden = new MySqlCommandBuilder(Adaptador);

                Adaptador.Fill(Resultado);
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo obtener la tabla la base de datos.\nError MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
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

        private SecureString CrearRutaDeAcceso(DatosDeConexion Seleccion, SecureString Usuario, SecureString Contrasena)
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

            if(Seleccion == null)
            {
                throw new ArgumentNullException("Seleccion");
            }
            else if(Usuario == null)
            {
                throw new ArgumentNullException("Usuario");
            }
            else if (Contrasena == null)
            {
                throw new ArgumentNullException("Contrasena");
            }

            List<string> RutaDeConexion = new List<string>();

            // 1) Nombre del servidor anfitrion
            RutaDeConexion.Add(string.Format("Host={0}", Seleccion.Anfitrion));

            // 2) Metodo de conexion
            switch (Seleccion.MetodoDeConexion)
            {
                case Constantes.MetodosDeConexion.TCP_IP:
                    RutaDeConexion.Add("Protocol=\"tcp\"");
                    RutaDeConexion.Add(string.Format("Port={0}", Seleccion.ArgumentoDeConexion));
                    break;
                case Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE:
                    RutaDeConexion.Add("Protocol=\"pipe\"");
                    RutaDeConexion.Add(string.Format("Pipe={0}", Seleccion.ArgumentoDeConexion));
                    break;
                case Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA:
                    RutaDeConexion.Add("Protocol=\"memory\"");
                    RutaDeConexion.Add(string.Format("Shared Memory Name={0}", Seleccion.ArgumentoDeConexion));
                    break;
                default:
                    break;
            }

            // 3) Requerimos pooling
            RutaDeConexion.Add("Pooling=true");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            RutaDeConexion.Add("Persist Security Info=false");

            // 5) Nombre de usuario
            RutaDeConexion.Add(("Username=" + Usuario.ConvertirAUnsecureString()));

            // 6) Contraseña
            RutaDeConexion.Add(("Password=" + Contrasena.ConvertirAUnsecureString()));

            /*
             * De la instruccion anterior (agregar password) hasta la siguiente (return) hay un hueco de 
             * seguridad porque cualquiera puede leer la contraseña al acceder a los miembros de RutaDeConexion
             */

            return (string.Join(";", RutaDeConexion.ToArray())).ConvertirASecureString();
        }
         
        #endregion

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return _Conexion.State; }
        }

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set { _Conexion.StateChange += value; }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena)
        {
            if (_Conexion != null)
                _Conexion.Close();

            try
            {
                _Conexion.ConnectionString = CrearRutaDeAcceso(Servidor, Usuario, Contrasena).ConvertirAUnsecureString();
                _Conexion.Open();
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

        void IBaseDeDatos.Desconectar()
        {
            if (_Conexion != null)
                _Conexion.Close();
        }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            return LectorSimple("SHOW DATABASES");
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            CambiarBaseDeDatos(BaseDeDatos);
            return LectorSimple("SHOW TABLES");
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            DataTable Descripcion = new DataTable();

            CambiarBaseDeDatos(BaseDeDatos);

            // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
            Descripcion = LectorAvanzado("DESCRIBE " + Tabla);
            List<string> ColumnasPermitidas = new List<string>();

            foreach (DataRow Fila in Descripcion.Rows)
            {
                ColumnasPermitidas.Add(Fila[0] as string);
            }

            string Columnas = string.Join(", ", ColumnasPermitidas.ToArray());

            /*
             * Ahora si seleccionamos solo las columnas visibles. Un SELECT * FROM podria 
             * generar un error si el usuario no tiene los privilegios suficientes
             */
            return LectorAvanzado("SELECT " + Columnas + " FROM " + Tabla);
        }

        object IBaseDeDatos.CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            object Resultado = null;
            string SQL = string.Empty;

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
                // 3) Chequeamos a ver si ya existe el usuario en el sistema
                SQL = "SELECT user FROM mysql.user WHERE user = '" + Usuario.ConvertirAUnsecureString() + "' AND host = 'localhost'";
                List<string> UsuariosExistentes = LectorSimple(SQL).ToList();
                
                // 4) Si es asi, se elimina
                if(UsuariosExistentes.Contains(Usuario.ConvertirAUnsecureString()))
                {
                    SQL = "DROP USER '" + Usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    EjecutarOrden(SQL);
                }
                
                // 5) Creamos el usuario con su contraseña
                SQL = "CREATE USER '" + Usuario.ConvertirAUnsecureString() + "'@'localhost' " +
                    "IDENTIFIED BY '" + Contrasena.ConvertirAUnsecureString() + "'";
                EjecutarOrden(SQL);

                // 4) Otorgamos los privilegios de columnas
                foreach (KeyValuePair<string, string> Par in ColumnasLista)
                {
                    SQL = "GRANT ";

                    for (int i = 0; i < PrivilegiosLista.Count; i++)
                    {
                        SQL += PrivilegiosLista[i] + " (" + Par.Value + ")";
                        if ((i + 1) < PrivilegiosLista.Count)
                        {
                            SQL += ", ";
                        }
                    }

                    SQL += " ON " + Par.Key + " TO '" + Usuario.ConvertirAUnsecureString() + "'@'localhost'";
                    EjecutarOrden(SQL);
                }

                // 5) Actualizamos la cache de privilegios del servidor
                EjecutarOrden("FLUSH PRIVILEGES");
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        #endregion

        #region Tipos anidados

        public static class OrdenesComunes
        {
            public static Dictionary<int,string> Privilegios = new Dictionary<int,string>() 
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
