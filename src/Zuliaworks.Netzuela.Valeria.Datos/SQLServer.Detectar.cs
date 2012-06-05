namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Win32;                              // RegistryKey, Registry
    using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal, ParametrosDeConexion, ExpresionGenerica
    
    /// <summary>
    /// 
    /// </summary>
    public partial class SQLServer
    {
        #region Variables y constantes

        private const string LOCALIZACION_SQL_SERVER_EN_REGISTRO = "SOFTWARE\\Microsoft\\Microsoft SQL Server";
        private const string LOCALIZACION_INSTANCIAS_SQL_SERVER_2000 = "SOFTWARE\\Microsoft\\MSQLServer\\MSSQLServer\\SuperSocketNetLib\\TCP";
        private const string LOCALIZACION_INSTANCIAS_SQL_SERVER_2005 = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";

        private const string CANALIZACIONES = "MSSQLServer\\SuperSocketNetLib\\Np";
        private const string MEMORIA_COMPARTIDA = "MSSQLServer\\SuperSocketNetLib\\Sm";
        private const string VIA = "MSSQLServer\\SuperSocketNetLib\\Via";
        private const string TCP_IP = "MSSQLServer\\SuperSocketNetLib\\Tcp";

        private delegate void DelegadoLeerPuertos(RegistryKey Registro, List<string> Puertos);
        
        #endregion

        #region Funciones

        public static ServidorLocal DetectarServidor()
        {
            /*
             * SQL Server:
             * ===========
             * 
             * La informacion sobre las instancias y los metodos de conexion de SQL Server se encuentra en el registro de
             * Windows en alguna de las siguientes ubicaciones:
             * 
             *  *- Los nombres de las instancias:
             *      HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names
             *  *- Metodos de conexion:
             *      HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Tcp
             *      HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Sm
             *      HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Np
             *      HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Via
             */

            List<ServidorLocal.Instancia> ListaDeInstancias = new List<ServidorLocal.Instancia>();

            /*
             * Por lo pronto no voy a habilitar la compatibilidad con SQL Server 2000
             * 
             * // Chequeamos a ver si esta instalado SQL Server 2000 
             * RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MSQLServer\\MSSQLServer\\SuperSocketNetLib\\TCP");
             */

            // Chequeamos a ver si esta instalada alguna de las versiones posteriores a SQL Server 2000
            //RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_INSTANCIAS_SQL_SERVER_2005);

            if (Registro != null)
            {
                string[] InstanciasRegistradas = Registro.GetValueNames();

                foreach (string NombreSimpleInstancia in InstanciasRegistradas)
                {
                    if (NombreSimpleInstancia == string.Empty)
                    {
                        continue;
                    }

                    ServidorLocal.Instancia Instancia = new ServidorLocal.Instancia();
                    List<ServidorLocal.MetodoDeConexion> Metodos = new List<ServidorLocal.MetodoDeConexion>();
                    ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();

                    string NombreLegalInstancia = (string)Registro.GetValue(NombreSimpleInstancia);
                    Instancia.Nombre = NombreSimpleInstancia;

                    // Canalizaciones por nombre (a.k.a tuberias)
                    Metodo = DetectarCanalizaciones(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                    {
                        Metodos.Add(Metodo);
                    }

                    // Memoria compartida
                    Metodo = DetectarMemoriaCompartida(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                    {
                        Metodos.Add(Metodo);
                    }

                    // VIA
                    Metodo = DetectarVIA(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                    {
                        Metodos.Add(Metodo);
                    }

                    // TCP/IP
                    Metodo = DetectarTCPIP(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                    {
                        Metodos.Add(Metodo);
                    }

                    Instancia.Metodos = Metodos;
                    ListaDeInstancias.Add(Instancia);
                }
            }

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = RDBMS.SqlServer;
            Serv.Instancias = ListaDeInstancias;

            return Serv;
        }

        private static ServidorLocal.MetodoDeConexion DetectarCanalizaciones(string nombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion metodo;
            RegistryKey registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + CANALIZACIONES);

            if (registro != null)
            {
                int valor1 = (int)registro.GetValue("Enabled");
                string valor2 = (string)registro.GetValue("PipeName");

                if (valor1 == 0x01 && valor2 != null)
                {
                    valor2 = valor2.Replace(" ", string.Empty);
                    string[] Valores = valor2.Split(',').ToArray();

                    metodo.Nombre = MetodosDeConexion.CanalizacionesConNombre;
                    metodo.Valores = Valores.ToList();
                }
            }

            return metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarMemoriaCompartida(string nombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion metodo;
            RegistryKey registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + MEMORIA_COMPARTIDA);
            
            if (registro != null)
            {
                int valor1 = (int)registro.GetValue("Enabled");
                if (valor1 == 0x01)
                {
                    metodo.Nombre = MetodosDeConexion.MemoriaCompartida;
                    metodo.Valores = new List<string>() { "Por defecto" };                    
                }
            }

            return metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarVIA(string nombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion metodo;
            RegistryKey registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + VIA);

            if (registro != null)
            {
                int valor1 = (int)registro.GetValue("Enabled");
                string valor2 = (string)registro.GetValue("ListenInfo");

                if (valor1 == 0x01 && valor2 != null)
                {
                    valor2 = valor2.Replace(" ", string.Empty);
                    string[] Valores = valor2.Split(',').ToArray();

                    metodo.Nombre = MetodosDeConexion.Via;
                    metodo.Valores = Valores.ToList();
                }
            }

            return metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarTCPIP(string nombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion metodo;

            DelegadoLeerPuertos leerPuertos = (r, v) =>
            {
                string valor2 = (string)r.GetValue("TcpDynamicPorts");
                string valor3 = (string)r.GetValue("TcpPort");

                valor2 = string.Concat(valor2, valor3);
                valor2 = valor2.Replace(" ", string.Empty);
                string[] arregloDeValores = valor2.Split(',');

                foreach (string va in arregloDeValores)
                {
                    if (va != string.Empty)
                    {
                        v.Add(va);
                    }
                }
            };

            RegistryKey registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + TCP_IP);

            if (registro != null)
            {
                int habilitado = (int)registro.GetValue("Enabled");
                if (habilitado == 0x01)
                {
                    List<string> valores = new List<string>();

                    int escucharTodasLasIP = (int)registro.GetValue("ListenOnAllIPs");
                    if (escucharTodasLasIP == 0x01)
                    {
                        registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + TCP_IP + "\\IPAll");
                        if (registro != null)
                        {
                            leerPuertos(registro, valores);
                        }
                    }
                    else
                    {
                        string[] IPs = registro.GetSubKeyNames();
                        foreach (string IP in IPs)
                        {
                            registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + nombreLegalInstancia + "\\" + TCP_IP + "\\" + IP);
                            if (registro != null)
                            {
                                leerPuertos(registro, valores);
                            }
                        }
                    }

                    metodo.Nombre = MetodosDeConexion.TcpIp;
                    metodo.Valores = valores.ToList();
                }
            }

            return metodo;
        }
        
        #endregion
    }
}
