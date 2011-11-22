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
                }
            }

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.MYSQL;
            Serv.Instancias = Instancias;
            return Serv;
        }
    }
}
