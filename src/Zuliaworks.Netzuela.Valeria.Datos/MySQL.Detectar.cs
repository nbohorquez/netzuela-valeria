namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.IO;                                // StreamReader
    using System.Linq;
    using System.Text;

    using Microsoft.Win32;                          // Registry
    using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

    /// <summary>
    /// 
    /// </summary>
    public partial class MySQL
    {
        #region Constantes

        private const string LOCALIZACION_MYSQL_EN_REGISTRO = "SOFTWARE\\MySQL AB";
        private const string NOMBRE_MYSQL_EN_REGISTRO = "MySQL Server";
        private const string ARCHIVO_DE_CONFIGURACION = "my.ini";
        private const string SECCION_DEL_SERVIDOR = "[mysqld";
        private const string DESHABILITAR_TCPIP = "skip-networking";
        private const string HABILITAR_CANALIZACIONES = "enable-named-pipe";
        private const string MEMORIA_COMPARTIDA = "shared-memory-base-name=";
        private const string MEMORIA_COMPARTIDA_POR_DEFECTO = "MYSQL";
        private const string HABILITAR_MEMORIA_COMPARTIDA = "shared-memory";
        private const string PUERTO = "port=";
        private const string CANALIZACIONES = "socket=";        
        private const char CARACTER_DE_COMENTARIO = '#';

        #endregion

        #region Funciones

        /// <summary>
        /// Esta función detecta las instancias de servidores MySQL instalados en el sistema.
        /// </summary>
        /// <returns>Instancias MySQL detectadas.</returns>
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

            string[] archivosDeConfiguracion = DescubrirRutasDeInstalacion();

            /*
             * Se analiza sintacticamente ("parse" en ingles) cada archivo de configuracion para detectar
             * instancias y metodos de conexion. Las opciones que pueden aparecer en estos archivos se 
             * especifican con detalle en:
             * http://dev.mysql.com/doc/refman/5.5/en/connecting.html
             */

            List<ServidorLocal.Instancia> instancias = DetectarInstanciasInstaladas(archivosDeConfiguracion).ToList();

            ServidorLocal serv = new ServidorLocal();
            serv.Nombre = SGBDR.MySQL;
            serv.Instancias = instancias;
            return serv;
        }

        private static string[] DescubrirRutasDeInstalacion()
        {
            List<string> Rutas = new List<string>();

            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_MYSQL_EN_REGISTRO);

            if (Registro != null)
            {
                string[] Directorio = Registro.GetSubKeyNames();

                foreach (string s in Directorio)
                {
                    if (s.Contains(NOMBRE_MYSQL_EN_REGISTRO))
                    {
                        Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_MYSQL_EN_REGISTRO + "\\" +  s);
                        string Ruta = (string)Registro.GetValue("Location");

                        if (Ruta != null)
                        {
                            Rutas.Add(Ruta);
                        }
                    }
                }
            }

            return Rutas.ToArray();
        }

        private static ServidorLocal.MetodoDeConexion DetectarMetodo(string linea, string metodoDeConexion)
        {
            ServidorLocal.MetodoDeConexion metodo = new ServidorLocal.MetodoDeConexion();
            string argumento = linea.Replace(metodoDeConexion, string.Empty);

            if (metodoDeConexion == MEMORIA_COMPARTIDA && argumento == string.Empty)
            {
                argumento = MEMORIA_COMPARTIDA_POR_DEFECTO;
            }

            string[] argumentos = argumento.Split(',').ToArray();

            switch (metodoDeConexion)
            {
                case MEMORIA_COMPARTIDA:
                    metodo.Nombre = MetodosDeConexion.MemoriaCompartida;
                    break;
                case PUERTO:
                    metodo.Nombre = MetodosDeConexion.TcpIp;
                    break;
                case CANALIZACIONES:
                    metodo.Nombre = MetodosDeConexion.CanalizacionesConNombre;
                    break;
                default:
                    break;
            }
            
            metodo.Valores = argumentos.ToList();

            return metodo;
        }

        private static ServidorLocal.Instancia[] DetectarInstanciasInstaladas(string[] archivosDeConfiguracion)
        {
            List<ServidorLocal.Instancia> instancias = new List<ServidorLocal.Instancia>();

            foreach (string ruta in archivosDeConfiguracion)
            {
                try
                {
                    ServidorLocal.Instancia ins = new ServidorLocal.Instancia();

                    string linea;
                    string nombreDeInstancia = null;
                    bool seccionServidor = false;
                    bool memoriaCompartidaHabilitada = false;
                    bool canalizacionesHabilitadas = false;
                    bool tcpIpHabilitado = true;

                    StreamReader my_ini = new StreamReader(ruta + ARCHIVO_DE_CONFIGURACION);

                    while (my_ini.Peek() > 0)
                    {
                        linea = my_ini.ReadLine();

                        //Quitamos todos los espacios en blanco para analizar mejor
                        linea = linea.Replace(" ", string.Empty);

                        // Si esta linea esta comentada, pasamos a la siguiente
                        if (linea.Length == 0 || linea[0] == CARACTER_DE_COMENTARIO)
                        {
                            continue;
                        }

                        /*
                         * Si se encuentra "[mysqld" (puede ser [mysqld1], [mysqld2], [mysqld3], etc...)
                         * significa que hemos llegado a la seccion que especifica los datos del servidor
                         */
                        if (linea.Contains(SECCION_DEL_SERVIDOR))
                        {
                            nombreDeInstancia = linea.Replace("[", string.Empty);
                            nombreDeInstancia = nombreDeInstancia.Replace("]", string.Empty);

                            ins.Nombre = nombreDeInstancia;
                            ins.Metodos = new List<ServidorLocal.MetodoDeConexion>();
                            instancias.Add(ins);

                            seccionServidor = true;
                            continue;
                        }

                        if (seccionServidor)
                        {
                            ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();

                            if (linea.Contains(PUERTO) && tcpIpHabilitado)
                            {
                                Metodo = DetectarMetodo(linea, PUERTO);

                                ins = instancias[instancias.Count - 1];
                                ins.Metodos.Add(Metodo);
                                instancias[instancias.Count - 1] = ins;
                            }
                            else if (linea.Contains(DESHABILITAR_TCPIP))
                            {
                                tcpIpHabilitado = false;
                                ins = instancias[instancias.Count - 1];

                                for (int i = 0; i < ins.Metodos.Count; i++)
                                {
                                    if (ins.Metodos[i].Nombre == MetodosDeConexion.TcpIp)
                                    {
                                        ins.Metodos.RemoveAt(i);
                                        instancias[instancias.Count - 1] = ins;
                                    }
                                }
                            }
                            else if (linea.Contains(CANALIZACIONES) && canalizacionesHabilitadas)
                            {
                                Metodo = DetectarMetodo(linea, CANALIZACIONES);

                                ins = instancias[instancias.Count - 1];
                                ins.Metodos.Add(Metodo);
                                instancias[instancias.Count - 1] = ins;
                            }
                            else if (linea.Contains(HABILITAR_CANALIZACIONES))
                            {
                                canalizacionesHabilitadas = true;
                            }
                            else if (linea.Contains(MEMORIA_COMPARTIDA) && memoriaCompartidaHabilitada)
                            {
                                Metodo = DetectarMetodo(linea, MEMORIA_COMPARTIDA);

                                ins = instancias[instancias.Count - 1];
                                ins.Metodos.Add(Metodo);
                                instancias[instancias.Count - 1] = ins;
                            }
                            else if (linea.Contains(HABILITAR_MEMORIA_COMPARTIDA))
                            {
                                memoriaCompartidaHabilitada = true;
                            }
                        }
                    }

                    my_ini.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocurrio un error tratando de leer el archivo de configuracion de MySQL", ex);
                }
            }

            return instancias.ToArray();
        }

        #endregion
    }        
}
