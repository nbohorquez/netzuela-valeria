using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;                                  // ConnectionState, DataTable
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos.Web;        // ProxyDinamico

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos de Netzuela en Internet
    /// </summary>
    public class Netzuela : EventosComunes, IBaseDeDatos
    {
        #region Variables

        private ClienteSpuria _Cliente;

        // ¡Temporal!
        private ConnectionState _Estado;

        #endregion

        #region Constructores

        public Netzuela(ParametrosDeConexion ServidorBD)
        {
            DatosDeConexion = ServidorBD;

            // El argumento de ClienteValeria debe estar relacionado con DatosDeConexion
            _Cliente = new ClienteSpuria();

            // Inicializamos los manejadores de eventos
            _Cliente.ListarBasesDeDatosCompletado -= base.ManejarListarBasesDeDatosCompletado;
            _Cliente.ListarBasesDeDatosCompletado += base.ManejarListarBasesDeDatosCompletado;

            _Cliente.ListarTablasCompletado -= base.ManejarListarTablasCompletado;
            _Cliente.ListarTablasCompletado += base.ManejarListarTablasCompletado;

            _Cliente.LeerTablaCompletado -= base.ManejarLeerTablaCompletado;
            _Cliente.LeerTablaCompletado += base.ManejarLeerTablaCompletado;

            _Cliente.EscribirTablaCompletado -= base.ManejarEscribirTablaCompletado;
            _Cliente.EscribirTablaCompletado += base.ManejarEscribirTablaCompletado;

            _Cliente.CrearUsuarioCompletado -= base.ManejarCrearUsuarioCompletado;
            _Cliente.CrearUsuarioCompletado += base.ManejarCrearUsuarioCompletado;
            
            // Hay que ver como quito este pedazo de codigo tan feo
            _Estado = ConnectionState.Closed;
        }

        ~Netzuela()
        {
            Dispose(false);
        }

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;
            
            if (BorrarCodigoAdministrado)
            {
                this._Cliente.Dispose();
                this._Cliente = null;
            }
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return _Estado; }
            private set
            {
                if(value != _Estado)
                {
                    ConnectionState Anterior = _Estado;
                    _Estado = value;
                    DispararCambioDeEstado(new StateChangeEventArgs(Anterior, _Estado));
                }
            }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion

        #region Funciones

        #region Métodos sincrónicos

        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            try
            {
                Desconectar();

                _Cliente.UriWsdlServicio = DatosDeConexion.Anfitrion;
                _Cliente.Armar();
                
                // Esto hay que borrarlo
                Estado = ConnectionState.Open;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al establecer la conexión con el servidor de Netzuela", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                if (_Cliente != null)
                    _Cliente.Desconectar();
                
                // Esto hay que borrarlo
                Estado = ConnectionState.Closed;
            }
            catch(Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con el servidor de Netzuela", ex);
            }
        }

        public string[] ListarBasesDeDatos()
        {
            List<string> Resultado = new List<string>();

            try
            {
                Resultado = _Cliente.ListarBasesDeDatos().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
            
            return Resultado.ToArray();
        }

        public string[] ListarTablas(string BaseDeDatos)
        {
            List<string> Resultado = new List<string>();

            try
            {
                Resultado = _Cliente.ListarTablas(BaseDeDatos).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return Resultado.ToArray();
        }

        public DataTable LeerTabla(string BaseDeDatos, string Tabla)
        {            
            DataTable Resultado = new DataTable();

            try
            {
                Resultado = _Cliente.LeerTabla(BaseDeDatos, Tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return Resultado;
        }

        public bool EscribirTabla(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            bool Resultado = false;

            try
            {
                Resultado = _Cliente.EscribirTabla(BaseDeDatos, NombreTabla, Tabla);
            }
            catch (Exception ex)
            {
                string Error = "Error al escribir la tabla " + NombreTabla + " en la base de datos " + BaseDeDatos;
                throw new Exception(Error, ex);
            }

            return Resultado;
        }

        public bool CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarBasesDeDatosAsinc()
        {
            try
            {
                _Cliente.ListarBasesDeDatosAsinc();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
        }

        public void ListarTablasAsinc(string BaseDeDatos)
        {
            try
            {
                _Cliente.ListarTablasAsinc(BaseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }
        }

        public void LeerTablaAsinc(string BaseDeDatos, string Tabla)
        {
            try
            {
                _Cliente.LeerTablaAsinc(BaseDeDatos, Tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla", ex);
            }
        }

        public void EscribirTablaAsinc(string BaseDeDatos, string NombreTabla, DataTable Tabla)
        {
            try
            {
                _Cliente.EscribirTablaAsinc(BaseDeDatos, NombreTabla, Tabla);
            }
            catch (Exception ex)
            {
                string Error = "Error al escribir la tabla " + NombreTabla + " en la base de datos " + BaseDeDatos;
                throw new Exception(Error, ex);
            }
        }

        public void CrearUsuarioAsinc(SecureString Usuario, SecureString Contrasena, string[] Columnas, int Privilegios)
        {
            throw new NotImplementedException();
        }

        public void ConsultarAsinc(string baseDeDatos, string Sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
