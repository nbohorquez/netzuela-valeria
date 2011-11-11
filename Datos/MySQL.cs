using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;                  // Registry
using MySql.Data.MySqlClient;           // MySqlConnection
using System.ComponentModel;            // INotifyPropertyChanged
using System.Data;                      // ConnectionState, DataTable
using System.IO;                        // StreamReader
using System.Security;                  // SecureString

using Zuliaworks.Netzuela.Valeria.Comunes;                  // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos MySQL
    /// </summary>
    public class MySQL : IBaseDeDatos
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

        /*
        private void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            Conectar(CrearRutaDeAcceso(Servidor, Usuario, Contrasena));
        }
        */

        private void Conectar(SecureString RutaDeAcceso)
        {
            if (_Conexion != null)
                _Conexion.Close();

            try
            {
                _Conexion.ConnectionString = RutaDeAcceso.ConvertirAUnsecureString();
                //_Conexion = new MySqlConnection(RutaDeAcceso.ConvertirAUnsecureString());
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
                        throw new Exception("Error en la conexion", ex);
                }
            }
        }

        private string[] Listar(string SQL)
        {
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
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener la lista de elementos", ex);
                //MessageBox.Show("No se pudo obtener la lista de elementos: " + ex.Message + ex.InnerException);
            }
            finally
            {
                if (Lector != null)
                    Lector.Close();
            }

            return Resultado.ToArray();
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

        /*
        private SecureString CrearRutaDeAcceso(DatosDeConexion Seleccion)
        {
            // Pedimos nombre de usuario y contraseña
            VentanaAutentificacion Credenciales = new VentanaAutentificacion();
            Credenciales.ShowDialog();

            return (CrearRutaDeAcceso(Seleccion, Credenciales.txt_Usuario.Text.ConvertirASecureString(), Credenciales.pwd_Contasena.SecurePassword));
        }
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

            // 3) Base de datos inicial
            RutaDeConexion.Add("Database=mysql");

            // 4) Requerimos pooling
            RutaDeConexion.Add("Pooling=true");

            // 5) Aumentamos la seguridad no permitiendo que se pueda leer la ruta de acceso
            RutaDeConexion.Add("Persist Security Info=false");

            // 6) Nombre de usuario
            RutaDeConexion.Add(("Username=" + Usuario.ConvertirAUnsecureString()));

            // 7) Contraseña
            RutaDeConexion.Add(("Password=" + Contrasena.ConvertirAUnsecureString()));

            /*
             * De la instruccion anterior (agregar password) hasta la siguiente (return) hay un hueco de 
             * seguridad porque cualquiera puede leer la contraseña al acceder a los miembros de RutaDeConexion
             */

            return (string.Join(";", RutaDeConexion.ToArray())).ConvertirASecureString();
        }

        public static ServidorLocal DetectarServidor()
        {
            /*
             * MySQL:
             * ======
             * 
             * La informacion sobre el puerto de escucha de MySQL se encuentra en el archivo my.ini de configuración 
             * al lado de la etiqueta 'port=' en la seccion del servidor [mysqld]. El archivo my.ini reside en la carpeta
             * de instalación del servidor (su ubicación esta disponible en el registro de Windows).
             */

            // Se descubre la ruta de instalación de todos los servidores MySQL registrados en el sistema
            List<string> Rutas = new List<string>();
            string[] ArchivosDeConfiguracion;

            RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB");
            if (Registro != null)
            {
                string[] Directorio = Registro.GetSubKeyNames();

                foreach (string s in Directorio)
                {
                    if (s.Contains("MySQL Server"))
                    {
                        Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB\\" + s);
                        string Ruta = (string)Registro.GetValue("Location");

                        if (Ruta != null)
                        {
                            Rutas.Add(Ruta);
                        }
                    }
                }
            }

            ArchivosDeConfiguracion = Rutas.ToArray();

            /*
             * Se analiza sintacticamente ("parse" en ingles) cada archivo de configuracion para detectar
             * instancias y metodos de conexion. Las opciones que pueden aparecer en estos archivos se 
             * especifican con detalle en:
             * http://dev.mysql.com/doc/refman/5.5/en/connecting.html
             */

            List<ServidorLocal.Instancia> Instancias = new List<ServidorLocal.Instancia>();

            foreach (string Ruta in ArchivosDeConfiguracion)
            {
                try
                {
                    ServidorLocal.Instancia Ins = new ServidorLocal.Instancia();

                    string Linea;
                    string NombreDeInstancia = null;
                    bool SeccionServidor = false;
                    bool MemoriaCompartidaHabilitada = false;
                    bool CanalizacionesHabilitadas = false;
                    bool TcpIpHabilitado = true;

                    StreamReader my_ini = new StreamReader(Ruta + "my.ini");

                    while (my_ini.Peek() > 0)
                    {
                        Linea = my_ini.ReadLine();

                        //Quitamos todos los espacios en blanco para analizar mejor
                        Linea = Linea.Replace(" ", string.Empty);

                        // Si esta linea esta comentada, pasamos a la siguiente
                        if (Linea.Length == 0 || Linea[0] == '#')
                            continue;

                        /*
                         * Si se encuentra "[mysqld" (puede ser [mysqld1], [mysqld2], [mysqld3], etc...)
                         * significa que hemos llegado a la seccion que especifica los datos del servidor
                         */
                        if (Linea.Contains("[mysqld"))
                        {
                            NombreDeInstancia = Linea.Replace("[", string.Empty);
                            NombreDeInstancia = NombreDeInstancia.Replace("]", string.Empty);

                            Ins.Nombre = NombreDeInstancia;
                            Ins.Metodos = new List<ServidorLocal.MetodoDeConexion>();
                            Instancias.Add(Ins);

                            SeccionServidor = true;
                            continue;
                        }

                        if (SeccionServidor)
                        {
                            ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();

                            // TCP/IP
                            if (Linea.Contains("port=") && TcpIpHabilitado)
                            {
                                string Puerto = Linea.Replace("port=", string.Empty);
                                // Esta instruccion esta demas porque MySQL no puede escuchar mas de un puerto a la vez...
                                string[] Puertos = Puerto.Split(',').ToArray();

                                Metodo.Nombre = Constantes.MetodosDeConexion.TCP_IP;
                                Metodo.Valores = Puertos.ToList();

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }
                            // Esto deshabilita TCP/IP
                            else if (Linea.Contains("skip-networking"))
                            {
                                TcpIpHabilitado = false;
                                Ins = Instancias[Instancias.Count - 1];

                                for (int i = 0; i < Ins.Metodos.Count; i++)
                                {
                                    if (Ins.Metodos[i].Nombre == Constantes.MetodosDeConexion.TCP_IP)
                                    {
                                        Ins.Metodos.RemoveAt(i);
                                        Instancias[Instancias.Count - 1] = Ins;
                                    }
                                }
                            }
                            // Canalizaciones con nombre
                            else if (Linea.Contains("socket=") && CanalizacionesHabilitadas)
                            {
                                string Socket = Linea.Replace("socket=", string.Empty);
                                // Esta instruccion esta demas porque MySQL no puede escuchar mas de un socket a la vez...
                                string[] Sockets = Socket.Split(',').ToArray();

                                Metodo.Nombre = Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE;
                                Metodo.Valores = Sockets.ToList();

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }

                            // Esto habilita las canalizaciones con nombre
                            else if (Linea.Contains("enable-named-pipe"))
                            {
                                CanalizacionesHabilitadas = true;
                            }
                            // Memoria compartida
                            else if (Linea.Contains("shared-memory-base-name=") && MemoriaCompartidaHabilitada)
                            {
                                string DireccionDeMemoria = Linea.Replace("shared-memory-base-name=", string.Empty);

                                // Si no se especifica una direccion de memoria, "MYSQL" se coloca por defecto
                                if (DireccionDeMemoria == string.Empty)
                                    DireccionDeMemoria = "MYSQL";

                                // Esta instruccion esta demas porque MySQL no puede escuchar mas de un socket a la vez...
                                string[] DireccionesDeMemorias = DireccionDeMemoria.Split(',').ToArray();

                                Metodo.Nombre = Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA;
                                Metodo.Valores = DireccionesDeMemorias.ToList();

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }
                            // Esto habilita la memoria compartida
                            else if (Linea.Contains("shared-memory"))
                            {
                                MemoriaCompartidaHabilitada = true;
                            }
                        }
                    }
                    my_ini.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocurrio un error tratando de leer el archivo de configuracion de MySQL", ex);
                    //MessageBox.Show("Ocurrio un error tratando de leer el archivo de configuracion de MySQL" + e.Message);
                }
            }

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.MYSQL;
            Serv.Instancias = Instancias;
            return Serv;
        }
         
        #endregion

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return _Conexion.State; }
        }

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set
            {
                _Conexion.StateChange += value;
            }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena)
        {
            Conectar(CrearRutaDeAcceso(Servidor, Usuario, Contrasena));
        }

        void IBaseDeDatos.Desconectar()
        {
            if (_Conexion != null)
                _Conexion.Close();
        }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            return Listar(OrdenesComunes.LISTAR_BASES_DE_DATOS);
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            string[] Resultado = null;

            try
            {
                _Conexion.ChangeDatabase(BaseDeDatos);
                Resultado = Listar(OrdenesComunes.LISTAR_TABLAS);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener la lista de elementos", ex);
                //MessageBox.Show("No se pudo obtener la lista de elementos: " + ex.Message + ex.InnerException);
            }
            
            return Resultado;
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            DataTable Datos = new DataTable();

            try
            {
                MySqlDataAdapter Adaptador;
                MySqlCommandBuilder CreadorDeOrden;

                if (_Conexion.Database != BaseDeDatos)
                    _Conexion.ChangeDatabase(BaseDeDatos);

                Adaptador = new MySqlDataAdapter(OrdenesComunes.MOSTRAR_TODO + Tabla, _Conexion);
                CreadorDeOrden = new MySqlCommandBuilder(Adaptador);

                Adaptador.Fill(Datos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error cargando la tabla", ex);
                //MessageBox.Show("Error cargando la tabla: " + ex.Message + ex.InnerException);
            }

            return Datos;
        }

        #endregion

        #region Tipos anidados

        public static class OrdenesComunes
        {
            public const string LISTAR_BASES_DE_DATOS = "SHOW DATABASES";
            public const string LISTAR_TABLAS = "SHOW TABLES";
            public const string MOSTRAR_TODO = "SELECT * FROM ";
        }

        #endregion        
    }
}
