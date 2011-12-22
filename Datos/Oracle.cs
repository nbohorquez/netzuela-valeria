using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos Oracle
    /// </summary>
    public class Oracle : IBaseDeDatos
    {
        #region Variables

        private ConnectionState _Estado;

        #endregion

        #region Constructores

        public Oracle(ParametrosDeConexion ServidorBD) 
        {
            Servidor = ServidorBD;
        }

        #endregion

        #region Propiedades

        public ParametrosDeConexion Servidor { get; set; }

        #endregion

        #region Funciones

        public static ServidorLocal DetectarServidor()
        {
            /*
             * Oracle:
             * ======
             * 
             * Por hacer...
             */

            List<ServidorLocal.Instancia> Instancias = new List<ServidorLocal.Instancia>();

            ServidorLocal Serv = new ServidorLocal();
            Serv.Nombre = Constantes.SGBDR.ORACLE;
            Serv.Instancias = Instancias;
            return Serv;
        }

        #endregion

        #region Implementaciones de interfaces

        public ConnectionState Estado
        {
            get { return ConnectionState.Closed; }
        }

        public ParametrosDeConexion DatosDeConexion
        {
            get { return Servidor; }
        }

        public StateChangeEventHandler CambioDeEstado
        {
            set { throw new NotImplementedException(); }
        }

        public EventHandler<EventoEnviarTablasCompletadoArgs> EnviarTablasCompletado
        {
            set { throw new NotImplementedException(); }
        }
        
        public void Conectar(SecureString Usuario, SecureString Contrasena) { }

        public void Desconectar() { }

        public string[] ListarBasesDeDatos() 
        { 
            string[] Resultado = new string[] { };
            return Resultado;
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            string[] Resultado = new string[] { };
            return Resultado;
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {
            return new DataTable();
        }

        public void EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            throw new NotImplementedException();
        }

        public object CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
