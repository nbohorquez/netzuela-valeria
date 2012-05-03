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
    public interface IBaseDeDatos : IDisposable
    {
        #region Propiedades

        ConnectionState Estado { get; }
        ParametrosDeConexion DatosDeConexion { get; set; }
        
        #endregion

        #region Eventos

        event StateChangeEventHandler CambioDeEstado;
        event EventHandler<EventoListarBDsCompletadoArgs> ListarBasesDeDatosCompletado;
        event EventHandler<EventoListarTablasCompletadoArgs> ListarTablasCompletado;
        event EventHandler<EventoLeerTablaCompletadoArgs> LeerTablaCompletado;
        event EventHandler<EventoEscribirTablaCompletadoArgs> EscribirTablaCompletado;
        event EventHandler<EventoCrearUsuarioCompletadoArgs> CrearUsuarioCompletado;
        event EventHandler<EventoConsultarCompletadoArgs> ConsultarCompletado;

        #endregion

        #region Prototipos de funciones

        #region Métodos sincrónicos

        void Conectar(SecureString usuario, SecureString contrasena);
        void Desconectar();
        string[] ListarBasesDeDatos();
        string[] ListarTablas(string baseDeDatos);
        DataTable LeerTabla(string baseDeDatos, string tabla);
        bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla);
        bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios);
        DataTable Consultar(string baseDeDatos, string Sql);

        #endregion

        #region Métodos asincrónicos

        void ListarBasesDeDatosAsinc();
        void ListarTablasAsinc(string baseDeDatos);
        void LeerTablaAsinc(string baseDeDatos, string tabla);
        void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla);
        void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios);
        void ConsultarAsinc(string baseDeDatos, string Sql);

        #endregion

        #endregion
    }
}