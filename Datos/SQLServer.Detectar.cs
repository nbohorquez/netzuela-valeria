using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zuliaworks.Netzuela.Valeria.Comunes;          // ServidorLocal, ParametrosDeConexion
using Microsoft.Win32;                              // RegistryKey, Registry

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    public partial class SQLServer
    {
        #region Constantes

        private const string LOCALIZACION_SQL_SERVER_EN_REGISTRO = "SOFTWARE\\Microsoft\\Microsoft SQL Server";
        private const string LOCALIZACION_INSTANCIAS_SQL_SERVER_2000 = "SOFTWARE\\Microsoft\\MSQLServer\\MSSQLServer\\SuperSocketNetLib\\TCP";
        private const string LOCALIZACION_INSTANCIAS_SQL_SERVER_2005 = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";

        private const string CANALIZACIONES = "MSSQLServer\\SuperSocketNetLib\\Np";
        private const string MEMORIA_COMPARTIDA = "MSSQLServer\\SuperSocketNetLib\\Sm";
        private const string VIA = "MSSQLServer\\SuperSocketNetLib\\Via";
        private const string TCP_IP = "MSSQLServer\\SuperSocketNetLib\\Tcp";
        
        #endregion

        #region Funciones

        private static ServidorLocal.MetodoDeConexion DetectarCanalizaciones(string NombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion Metodo;

            //RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Np");
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + CANALIZACIONES);

            if (Registro != null)
            {
                int Valor1 = (int)Registro.GetValue("Enabled");
                string Valor2 = (string)Registro.GetValue("PipeName");

                if (Valor1 == 0x01 && Valor2 != null)
                {
                    Valor2 = Valor2.Replace(" ", string.Empty);
                    string[] Valores = Valor2.Split(',').ToArray();

                    Metodo.Nombre = Constantes.MetodosDeConexion.CANALIZACIONES_CON_NOMBRE;
                    Metodo.Valores = Valores.ToList();
                }
            }

            return Metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarMemoriaCompartida(string NombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion Metodo;

            //RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Sm");
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + MEMORIA_COMPARTIDA);
            
            if (Registro != null)
            {
                int Valor1 = (int)Registro.GetValue("Enabled");

                if (Valor1 == 0x01)
                {
                    Metodo.Nombre = Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA;
                    Metodo.Valores = null;                    
                }
            }

            return Metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarVIA(string NombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion Metodo;

            //RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Via");
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + VIA);

            if (Registro != null)
            {
                int Valor1 = (int)Registro.GetValue("Enabled");
                string Valor2 = (string)Registro.GetValue("ListenInfo");

                if (Valor1 == 0x01 && Valor2 != null)
                {
                    Valor2 = Valor2.Replace(" ", string.Empty);
                    string[] Valores = Valor2.Split(',').ToArray();

                    Metodo.Nombre = Constantes.MetodosDeConexion.VIA;
                    Metodo.Valores = Valores.ToList();
                }
            }

            return Metodo;
        }

        private static ServidorLocal.MetodoDeConexion DetectarTCPIP(string NombreLegalInstancia)
        {
            ServidorLocal.MetodoDeConexion Metodo;

            //RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp");
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + TCP_IP);

            if (Registro != null)
            {
                int Valor1 = (int)Registro.GetValue("Enabled");

                if (Valor1 == 0x01)
                {
                    string[] Valores = null;

                    Valor1 = (int)Registro.GetValue("ListenOnAllIPs");
                    if (Valor1 == 0x01)
                    {
                        //Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\IPAll");
                        Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + TCP_IP + "\\IPAll");
                        if (Registro != null)
                        {
                            string Valor2 = (string)Registro.GetValue("TcpDynamicPorts");
                            string Valor3 = (string)Registro.GetValue("TcpPort");

                            Valor2 = String.Concat(Valor2, Valor3);
                            Valor2 = Valor2.Replace(" ", string.Empty);
                            Valores = Valor2.Split(',').ToArray();
                        }
                    }
                    else
                    {
                        string[] IPs = Registro.GetSubKeyNames();

                        foreach (string IP in IPs)
                        {
                            //Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\" + IP);
                            Registro = Registry.LocalMachine.OpenSubKey(LOCALIZACION_SQL_SERVER_EN_REGISTRO + "\\" + NombreLegalInstancia + "\\" + TCP_IP + "\\" + IP);

                            if (Registro != null)
                            {
                                string Valor2 = (string)Registro.GetValue("IpAddress");
                                if (Valor2 == "127.0.0.1")
                                {
                                    Valor2 = (string)Registro.GetValue("TcpDynamicPorts");
                                    string Valor3 = (string)Registro.GetValue("TcpPort");

                                    Valor2 = String.Concat(Valor2, Valor3);
                                    Valor2 = Valor2.Replace(" ", string.Empty);
                                    Valores = Valor2.Split(',').ToArray();
                                }
                            }
                        }
                    }

                    Metodo.Nombre = Constantes.MetodosDeConexion.TCP_IP;
                    Metodo.Valores = Valores.ToList();
                }
            }

            return Metodo;
        }

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
                        continue;

                    ServidorLocal.Instancia Instancia = new ServidorLocal.Instancia();
                    List<ServidorLocal.MetodoDeConexion> Metodos = new List<ServidorLocal.MetodoDeConexion>();
                    ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();

                    string NombreLegalInstancia = (string)Registro.GetValue(NombreSimpleInstancia);
                    Instancia.Nombre = NombreSimpleInstancia;

                    // Canalizaciones por nombre (a.k.a tuberias)
                    Metodo = DetectarCanalizaciones(NombreLegalInstancia);
                    if(Metodo.Nombre != null)
                        Metodos.Add(Metodo);

                    // Memoria compartida
                    Metodo = DetectarMemoriaCompartida(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                        Metodos.Add(Metodo);

                    // VIA
                    Metodo = DetectarVIA(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                        Metodos.Add(Metodo);

                    // TCP/IP
                    Metodo = DetectarTCPIP(NombreLegalInstancia);
                    if (Metodo.Nombre != null)
                        Metodos.Add(Metodo);
                    
                    Instancia.Metodos = Metodos;
                    ListaDeInstancias.Add(Instancia);
                }
            }

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.SQL_SERVER;
            Serv.Instancias = ListaDeInstancias;

            return Serv;
        }
        
        #endregion
    }
}
