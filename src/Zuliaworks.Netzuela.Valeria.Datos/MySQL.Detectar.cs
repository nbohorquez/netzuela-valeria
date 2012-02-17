using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;                          // Registry
using System.IO;                                // StreamReader
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
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

        private static ServidorLocal.MetodoDeConexion DetectarMetodo(string Linea, string MetodoDeConexion)
        {
            ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();
            string Argumento = Linea.Replace(MetodoDeConexion, string.Empty);

            if (MetodoDeConexion == MEMORIA_COMPARTIDA && Argumento == string.Empty)
            {
                Argumento = MEMORIA_COMPARTIDA_POR_DEFECTO;
            }

            string[] Argumentos = Argumento.Split(',').ToArray();

            switch (MetodoDeConexion)
            {
                case MEMORIA_COMPARTIDA:
                    Metodo.Nombre = MetodosDeConexion.MemoriaCompartida;
                    break;
                case PUERTO:
                    Metodo.Nombre = MetodosDeConexion.TcpIp;
                    break;
                case CANALIZACIONES:
                    Metodo.Nombre = MetodosDeConexion.CanalizacionesConNombre;
                    break;
                default:
                    break;
            }
            
            Metodo.Valores = Argumentos.ToList();

            return Metodo;
        }

        private static ServidorLocal.Instancia[] DetectarInstanciasInstaladas(string[] ArchivosDeConfiguracion)
        {
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

                    StreamReader my_ini = new StreamReader(Ruta + ARCHIVO_DE_CONFIGURACION);

                    while (my_ini.Peek() > 0)
                    {
                        Linea = my_ini.ReadLine();

                        //Quitamos todos los espacios en blanco para analizar mejor
                        Linea = Linea.Replace(" ", string.Empty);

                        // Si esta linea esta comentada, pasamos a la siguiente
                        if (Linea.Length == 0 || Linea[0] == CARACTER_DE_COMENTARIO)
                            continue;

                        /*
                         * Si se encuentra "[mysqld" (puede ser [mysqld1], [mysqld2], [mysqld3], etc...)
                         * significa que hemos llegado a la seccion que especifica los datos del servidor
                         */
                        if (Linea.Contains(SECCION_DEL_SERVIDOR))
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

                            #region TCP/IP

                            if (Linea.Contains(PUERTO) && TcpIpHabilitado)
                            {
                                Metodo = DetectarMetodo(Linea, PUERTO);

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }
                            // Esto deshabilita el TCP/IP
                            else if (Linea.Contains(DESHABILITAR_TCPIP))
                            {
                                TcpIpHabilitado = false;
                                Ins = Instancias[Instancias.Count - 1];

                                for (int i = 0; i < Ins.Metodos.Count; i++)
                                {
                                    if (Ins.Metodos[i].Nombre == MetodosDeConexion.TcpIp)
                                    {
                                        Ins.Metodos.RemoveAt(i);
                                        Instancias[Instancias.Count - 1] = Ins;
                                    }
                                }
                            }

                            #endregion

                            #region Canalizaciones con nombre

                            else if (Linea.Contains(CANALIZACIONES) && CanalizacionesHabilitadas)
                            {
                                Metodo = DetectarMetodo(Linea, CANALIZACIONES);

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }
                            // Esto habilita las canalizaciones con nombre
                            else if (Linea.Contains(HABILITAR_CANALIZACIONES))
                            {
                                CanalizacionesHabilitadas = true;
                            }

                            #endregion

                            #region Memoria compartida

                            else if (Linea.Contains(MEMORIA_COMPARTIDA) && MemoriaCompartidaHabilitada)
                            {
                                Metodo = DetectarMetodo(Linea, MEMORIA_COMPARTIDA);

                                Ins = Instancias[Instancias.Count - 1];
                                Ins.Metodos.Add(Metodo);
                                Instancias[Instancias.Count - 1] = Ins;
                            }
                            // Esto habilita la memoria compartida
                            else if (Linea.Contains(HABILITAR_MEMORIA_COMPARTIDA))
                            {
                                MemoriaCompartidaHabilitada = true;
                            }

                            #endregion
                        }
                    }

                    my_ini.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocurrio un error tratando de leer el archivo de configuracion de MySQL", ex);
                }
            }

            return Instancias.ToArray();
        }

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

            string[] ArchivosDeConfiguracion = DescubrirRutasDeInstalacion();

            /*
             * Se analiza sintacticamente ("parse" en ingles) cada archivo de configuracion para detectar
             * instancias y metodos de conexion. Las opciones que pueden aparecer en estos archivos se 
             * especifican con detalle en:
             * http://dev.mysql.com/doc/refman/5.5/en/connecting.html
             */

            List<ServidorLocal.Instancia> Instancias = DetectarInstanciasInstaladas(ArchivosDeConfiguracion).ToList();

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = SGBDR.MySQL;
            Serv.Instancias = Instancias;
            return Serv;
        }
        
        #endregion
    }        
}
