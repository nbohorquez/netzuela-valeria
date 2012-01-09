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
    public partial class SQLServer : IBaseDeDatos
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
                throw new Exception("Error al cambiar la base de datos.\nError MSSQL No. " + ex.Number.ToString(), ex);
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
                throw new Exception("No se pudo ejecutar la orden.\nError MSSQL No. " + ex.Number.ToString(), ex);
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
                throw new Exception("No se pudo obtener la lista de elementos desde la base de datos.\nError MSSQL No. " + ex.Number.ToString(), ex);
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
                throw new Exception("No se pudo obtener la tabla la base de datos.\nError MSSQL No. " + ex.Number.ToString(), ex);
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

            if ((Seleccion.ArgumentoDeConexion != null) && (Seleccion.ArgumentoDeConexion != string.Empty) && (Seleccion.ArgumentoDeConexion != "N/A"))
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

        #region Eventos

        public event StateChangeEventHandler CambioDeEstado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarBasesDeDatosCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> ListarTablasCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> LeerTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> EscribirTablaCompletado;
        public event EventHandler<EventoOperacionAsincCompletadaArgs> CrearUsuarioCompletado;

        #endregion

        #region Funciones

        #region Métodos de eventos

        private void ManejarCambioDeEstado(object Remitente, StateChangeEventArgs Args)
        {
            DispararCambioDeEstado(Args);
        }

        private void ManejarListarBasesDeDatosCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararListarBasesDeDatosCompletado(Args);
        }

        private void ManejarListarTablasCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararListarTablasCompletado(Args);
        }

        private void ManejarLeerTablaCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararLeerTablaCompletado(Args);
        }

        private void ManejarEscribirTablaCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararEscribirTablaCompletado(Args);
        }

        private void ManejarCrearUsuarioCompletado(object Remitente, EventoOperacionAsincCompletadaArgs Args)
        {
            DispararCrearUsuarioCompletado(Args);
        }

        protected virtual void DispararCambioDeEstado(StateChangeEventArgs e)
        {
            if (CambioDeEstado != null)
            {
                CambioDeEstado(this, e);
            }
        }

        protected virtual void DispararListarBasesDeDatosCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarBasesDeDatosCompletado != null)
            {
                ListarBasesDeDatosCompletado(this, e);
            }
        }

        protected virtual void DispararListarTablasCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (ListarTablasCompletado != null)
            {
                ListarTablasCompletado(this, e);
            }
        }

        protected virtual void DispararLeerTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (LeerTablaCompletado != null)
            {
                LeerTablaCompletado(this, e);
            }
        }

        protected virtual void DispararEscribirTablaCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (EscribirTablaCompletado != null)
            {
                EscribirTablaCompletado(this, e);
            }
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoOperacionAsincCompletadaArgs e)
        {
            if (CrearUsuarioCompletado != null)
            {
                CrearUsuarioCompletado(this, e);
            }
        }

        #endregion

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
                    /*case 0:
                        throw new Exception("No se puede conectar al servidor. Contacte al administrador", ex);*/
                    case 18456:
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
                _Conexion.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con la base de datos", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            //string[] ResultadoBruto = LectorSimple("SELECT name FROM sys.databases");
            string[] ResultadoBruto = LectorSimple("EXEC sp_databases");
            List<string> ResultadoFinal = new List<string>();

            foreach (string ResultadoParcial in ResultadoBruto)
                ResultadoFinal.Add(ResultadoParcial);

            /*
            // No podemos permitir que el usuario acceda a estas bases de datos privilegiadas
            for (int i = 0; i < ResultadoBruto.Length; i++)
            {
                if (ResultadoBruto[i] != "information_schema" &&
                    ResultadoBruto[i] != "mysql" &&
                    ResultadoBruto[i] != "performance_schema")
                {
                    ResultadoFinal.Add(ResultadoBruto[i]);
                }
            }*/

            return ResultadoFinal.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            CambiarBaseDeDatos(BaseDeDatos);

            //DataTable Resultado = LectorAvanzado("SELECT * FROM information_schema.tables WHERE table_name = '" + BaseDeDatos + "'");
            DataTable Resultado = LectorAvanzado("EXEC sp_tables @table_type = \"'TABLE'\"");           
            List<string> Filas = new List<string>();

            foreach (DataRow Fila in Resultado.Rows)
            {
                // La columna numero 3 es la que tiene el nombre de la tabla
                Filas.Add((string)Fila.ItemArray[2]);
            }

            return Filas.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            CambiarBaseDeDatos(BaseDeDatos);

            // Tenemos que ver primero cuales son las columnas a las que tenemos acceso
            DataTable Descripcion = LectorAvanzado("EXEC sp_columns @table_name = " + Tabla);
            List<string> ColumnasPermitidas = new List<string>();

            foreach (DataRow Fila in Descripcion.Rows)
            {
                ColumnasPermitidas.Add(Fila[3] as string);
            }

            string Columnas = string.Join(", ", ColumnasPermitidas.ToArray());

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
            throw new NotImplementedException();
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
