using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;              // DataTable, ConnectionState
using System.Security;          // SecureString

using Zuliaworks.Netzuela.Valeria.Comunes;

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
        ParametrosDeConexion DatosDeConexion { get; set; }
        
        #endregion

        #region Eventos

        event StateChangeEventHandler CambioDeEstado;
        event EventHandler<EventoOperacionAsincCompletadaArgs> ListarBasesDeDatosCompletado;
        event EventHandler<EventoOperacionAsincCompletadaArgs> ListarTablasCompletado;
        event EventHandler<EventoOperacionAsincCompletadaArgs> LeerTablaCompletado;
        event EventHandler<EventoOperacionAsincCompletadaArgs> EscribirTablaCompletado;
        event EventHandler<EventoOperacionAsincCompletadaArgs> CrearUsuarioCompletado;

        #endregion

        #region Prototipos de funciones

        #region Métodos sincrónicos

        void Conectar(SecureString Usuario, SecureString Contrasena);
        void Desconectar();
        string[] ListarBasesDeDatos();
        string[] ListarTablas(string BaseDeDatos);
        DataTable LeerTabla(string BaseDeDatos, string Tabla);
        bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla);
        bool CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios);

        #endregion

        #region Métodos asincrónicos

        void ListarBasesDeDatosAsinc();
        void ListarTablasAsinc(string BaseDeDatos);
        void LeerTablaAsinc(string BaseDeDatos, string Tabla);
        void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla);
        void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios);

        #endregion

        #endregion
    }
}