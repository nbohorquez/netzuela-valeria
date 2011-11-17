using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zuliaworks.Netzuela.Valeria.Comunes;              // DatosDeConexion
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security;              // SecureString
using Microsoft.Win32;

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos SQLServer
    /// </summary>
    public class SQLServer : IBaseDeDatos
    {
        #region Variables
                
        private SqlConnection _Conexion;

        #endregion

        #region Constructores

        public SQLServer(DatosDeConexion ServidorBD)
        {
            Servidor = ServidorBD;
            _Conexion = new SqlConnection();
        }

        #endregion

        #region Propiedades

        public DatosDeConexion Servidor { get; set; }

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
            RegistryKey Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
            if (Registro != null)
            {
                string[] InstanciasRegistradas = Registro.GetValueNames();

                foreach (string NombreSimpleInstancia in InstanciasRegistradas)
                {
                    ServidorLocal.Instancia Instancia = new ServidorLocal.Instancia();
                    List<ServidorLocal.MetodoDeConexion> Metodos = new List<ServidorLocal.MetodoDeConexion>();
                    ServidorLocal.MetodoDeConexion Metodo = new ServidorLocal.MetodoDeConexion();

                    string NombreLegalInstancia = (string)Registro.GetValue(NombreSimpleInstancia);
                    Instancia.Nombre = NombreSimpleInstancia;

                    // Canalizaciones por nombre (a.k.a tuberias)
                    Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Np");
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

                            Metodos.Add(Metodo);
                        }
                    }

                    // Memoria compartida
                    Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Sm");
                    if (Registro != null)
                    {
                        int Valor1 = (int)Registro.GetValue("Enabled");

                        if (Valor1 == 0x01)
                        {
                            Metodo.Nombre = Constantes.MetodosDeConexion.MEMORIA_COMPARTIDA;
                            Metodo.Valores = null;

                            Metodos.Add(Metodo);
                        }
                    }

                    // VIA
                    Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Via");
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

                            Metodos.Add(Metodo);
                        }
                    }

                    // TCP/IP
                    Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp");
                    if (Registro != null)
                    {
                        int Valor1 = (int)Registro.GetValue("Enabled");

                        if (Valor1 == 0x01)
                        {
                            string[] Valores = null;

                            Valor1 = (int)Registro.GetValue("ListenOnAllIPs");
                            if (Valor1 == 0x01)
                            {
                                Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\IPAll");
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
                                    Registro = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + NombreLegalInstancia + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\" + IP);
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

                            Metodos.Add(Metodo);
                        }
                    }

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

        #region Implementaciones de interfaces

        ConnectionState IBaseDeDatos.Estado
        {
            get { return _Conexion.State; }
        }

        StateChangeEventHandler IBaseDeDatos.EnCambioDeEstado
        {
            set { _Conexion.StateChange += value; }
        }

        void IBaseDeDatos.Conectar(SecureString Usuario, SecureString Contrasena) { }

        void IBaseDeDatos.Desconectar() { }

        string[] IBaseDeDatos.ListarBasesDeDatos()
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        string[] IBaseDeDatos.ListarTablas(string BaseDeDatos)
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        DataTable IBaseDeDatos.MostrarTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        object IBaseDeDatos.CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
