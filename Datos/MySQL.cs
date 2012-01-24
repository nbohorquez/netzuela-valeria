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
    public partial class MySQL : EventosComunes, IBaseDeDatos
    {
        #region Variables y constantes

        protected static Dictionary<int, string> PrivilegiosAOrdenes = new Dictionary<int, string>() 
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

        private MySqlConnection _Conexion;
        
        #endregion

        #region Constructores

        public MySQL(ParametrosDeConexion ServidorBD)
        {
            DatosDeConexion = ServidorBD;

            _Conexion = new MySqlConnection();
            _Conexion.StateChange -= base.ManejarCambioDeEstado;
            _Conexion.StateChange += base.ManejarCambioDeEstado;
        }

        #endregion

        #region Funciones

        private void CambiarBaseDeDatos(string BaseDeDatos)
        {
            if (_Conexion.Database != BaseDeDatos)
                _Conexion.ChangeDatabase(BaseDeDatos);
        }

        private void EjecutarOrden(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");
            
            MySqlCommand Orden = new MySqlCommand(SQL, _Conexion);
            Orden.ExecuteNonQuery();
        }

        private string[] LectorSimple(string SQL)
        {
            if (SQL == null)
                throw new ArgumentNullException("SQL");

            MySqlDataReader Lector = null;
            List<string> Resultado = new List<string>();

            MySqlCommand Orden = new MySqlCommand(SQL, _Conexion);

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

            MySqlDataAdapter Adaptador = new MySqlDataAdapter(SQL, _Conexion);
            MySqlCommandBuilder CreadorDeOrden = new MySqlCommandBuilder(Adaptador);

            Adaptador.Fill(Resultado);

            return Resultado;
        }

        private string DescribirTabla(string Tabla)
        {
            DataTable Descripcion = LectorAvanzado("DESCRIBE " + Tabla);

            var ColumnasPermitidas = from D in Descripcion.AsEnumerable()
                                     select D.Field<string>("Field");

            return string.Join(", ", ColumnasPermitidas.ToArray());
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

        private SecureString CrearRutaDeAcceso(ParametrosDeConexion Seleccion, SecureString Usuario, SecureString Contrasena)
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
                throw new ArgumentNullException("Seleccion");

            if(Usuario == null)
                throw new ArgumentNullException("Usuario");
            
            if (Contrasena == null)
                throw new ArgumentNullException("Contrasena");

            SecureString RutaDeConexion = new SecureString();

            // 1) Nombre del servidor anfitrion
            RutaDeConexion.AgregarString("Host=" + Seleccion.Anfitrion + ";");

            // 2) Metodo de conexion
            switch (Seleccion.MetodoDeConexion)
            {
                case Constantes.MetodosDeConexion.TCP_IP:
                    RutaDeConexion.AgregarString("Protocol=\"tcp\";");
                    RutaDeConexion.AgregarString("Port=" + Seleccion.ArgumentoDeConexion + ";");
                    break;
                case Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE:
                    RutaDeConexion.AgregarString("Protocol=\"pipe\";");
                    RutaDeConexion.AgregarString("Pipe=" + Seleccion.ArgumentoDeConexion + ";");
                    break;
                case Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA:
                    RutaDeConexion.AgregarString("Protocol=\"memory\";");
                    RutaDeConexion.AgregarString("Shared Memory Name=" + Seleccion.ArgumentoDeConexion + ";");
                    break;
                default:
                    break;
            }

            // 3) Requerimos pooling
            RutaDeConexion.AgregarString("Pooling=false;");

            // 4) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            RutaDeConexion.AgregarString("Persist Security Info=false;");

            // 5) Nombre de usuario
            RutaDeConexion.AgregarString(("Username=" + Usuario.ConvertirAUnsecureString()) + ";");

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
                if (_Conexion != null)
                    _Conexion.Close();
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
                string[] ResultadoBruto = LectorSimple("SHOW DATABASES");

                ResultadoFinal = new List<string>();

                foreach (string R in ResultadoBruto)
                {
                    if(R != "information_schema" && R != "mysql" && R != "performance_schema")
                        ResultadoFinal.Add(R);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las bases de datos. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return ResultadoFinal.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = null;

            try
            {
                CambiarBaseDeDatos(BaseDeDatos);

                string[] ResultadoBruto = LectorSimple("SHOW TABLES");

                Resultado = new List<string>();

                foreach (string S in ResultadoBruto)
                    Resultado.Add(S);
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al listar las tablas. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            DataTable TablaLeida = null;

            try
            {
                CambiarBaseDeDatos(BaseDeDatos);
                /*
                 * Tenemos que ver primero cuales son las columnas a las que tenemos acceso. Un "SELECT * 
                 * FROM ..." podria generar un error si el usuario no tiene los privilegios suficientes.
                 */
                string Columnas = DescribirTabla(Tabla);
                TablaLeida = LectorAvanzado("SELECT " + Columnas + " FROM " + Tabla);
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al leer la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
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

                MySqlDataAdapter Adaptador = new MySqlDataAdapter("SELECT " + Columnas + " FROM " + NombreTabla, _Conexion);
                MySqlCommandBuilder CreadorDeOrden = new MySqlCommandBuilder(Adaptador);
                
                Adaptador.FillSchema(Temporal, SchemaType.Source);
                Adaptador.Fill(Temporal);

                Adaptador.InsertCommand = new MySqlCommand("Insertar");
                Adaptador.InsertCommand.CommandType = CommandType.StoredProcedure;
                Adaptador.UpdateCommand = new MySqlCommand("Actualizar");
                Adaptador.UpdateCommand.CommandType = CommandType.StoredProcedure;
                Adaptador.DeleteCommand = new MySqlCommand("Borrar");
                Adaptador.DeleteCommand.CommandType = CommandType.StoredProcedure;

                string VariableDeEntrada = string.Empty;

                MySqlParameter VariableDeEntradaSQL = new MySqlParameter("a_Parametros", VariableDeEntrada);
                VariableDeEntradaSQL.Direction = ParameterDirection.Input;

                Adaptador.InsertCommand.Parameters.Add(VariableDeEntradaSQL);
                Adaptador.UpdateCommand.Parameters.Add(VariableDeEntradaSQL);
                Adaptador.DeleteCommand.Parameters.Add(VariableDeEntradaSQL);

                Temporal.Merge(Tabla, false, MissingSchemaAction.Error);
                
                MySqlRowUpdatingEventHandler ActualizandoFila = (r, a) =>
                {
                    List<string> Parametros = new List<string>();

                    foreach (object Dato in a.Row.ItemArray)
                    {
                        Parametros.Add(Dato.ToString().Replace(",", "."));
                    }

                    VariableDeEntrada = string.Join(",", Parametros.ToArray());
                    a.Command.Parameters[0].Value = VariableDeEntrada;
                };
                
                MySqlRowUpdatedEventHandler FilaActualizada = (r, a) =>
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
                Adaptador.Update(Temporal.Select(null, null, DataViewRowState.Deleted));
                // Luego los modificados
                Adaptador.Update(Temporal.Select(null, null, DataViewRowState.ModifiedCurrent));
                // Y por ultimo los agregados
                Adaptador.Update(Temporal.Select(null, null, DataViewRowState.Added));

                Resultado = true;
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo escribir la tabla. Error MySQL No. " + ex.Number.ToString(), ex);
            }

            return Resultado;
        }

        public bool CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            bool Resultado = false;
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

                // 6) Otorgamos los privilegios de columnas
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

                // 7) Actualizamos la cache de privilegios del servidor
                EjecutarOrden("FLUSH PRIVILEGES");

                Resultado = true;
            }
            catch (MySqlException ex)
            {
                throw new Exception("No se pudo crear el usuario especificado. Error MySQL No. " + ex.Number.ToString(), ex);
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
    }
}
