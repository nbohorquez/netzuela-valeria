using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;              // DataTable, ConnectionState
using System.Security;          // SecureString
using MySql.Data.MySqlClient;   // MySqlConnection

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Provee una interfaz unica para comunicarse con todos los implementadores de acceso 
    /// a las bases de datos.
    /// </summary>    
    public interface IBaseDeDatos
    {
        #region Propiedades

        ConnectionState Estado { get; }
        StateChangeEventHandler EnCambioDeEstado { set; }

        #endregion

        #region Prototipos de funciones

        void Conectar(SecureString Usuario, SecureString Contrasena);
        void Desconectar();
        string[] ListarBasesDeDatos();
        string[] ListarTablas(string BaseDeDatos);
        DataTable LeerTabla(string BaseDeDatos, string Tabla);
        void EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla);
        object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios);
        
        #endregion
    }
}