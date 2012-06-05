namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                                  // ConnectionState, DataTable
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    using Zuliaworks.Netzuela.Valeria.Datos.Web;        // ProxyDinamico

    /// <summary>
    /// Implementa las funciones de acceso a las bases de datos de Netzuela en Internet
    /// </summary>
    public class Netzuela : EventosComunes, INetzuela
    {
        #region Variables

        private ClienteSpuria cliente;
        private ConnectionState estado;         // ¡Temporal!

        #endregion

        #region Constructores

        public Netzuela(ParametrosDeConexion servidorBd)
        {
            this.DatosDeConexion = servidorBd;

            // El argumento de ClienteValeria debe estar relacionado con DatosDeConexion
            this.cliente = new ClienteSpuria();

            // Inicializamos los manejadores de eventos
            this.cliente.ListarTiendasCompletado -= this.ManejarListarTiendasCompletado;
            this.cliente.ListarTiendasCompletado += this.ManejarListarTiendasCompletado;

            this.cliente.ListarBasesDeDatosCompletado -= this.ManejarListarBasesDeDatosCompletado;
            this.cliente.ListarBasesDeDatosCompletado += this.ManejarListarBasesDeDatosCompletado;

            this.cliente.ListarTablasCompletado -= this.ManejarListarTablasCompletado;
            this.cliente.ListarTablasCompletado += this.ManejarListarTablasCompletado;

            this.cliente.LeerTablaCompletado -= this.ManejarLeerTablaCompletado;
            this.cliente.LeerTablaCompletado += this.ManejarLeerTablaCompletado;

            this.cliente.EscribirTablaCompletado -= this.ManejarEscribirTablaCompletado;
            this.cliente.EscribirTablaCompletado += this.ManejarEscribirTablaCompletado;

            this.cliente.CrearUsuarioCompletado -= this.ManejarCrearUsuarioCompletado;
            this.cliente.CrearUsuarioCompletado += this.ManejarCrearUsuarioCompletado;

            this.cliente.ConsultarCompletado -= this.ManejarConsultarCompletado;
            this.cliente.ConsultarCompletado += this.ManejarConsultarCompletado;

            // Hay que ver como quito este pedazo de codigo tan feo
            this.estado = ConnectionState.Closed;
        }

        ~Netzuela()
        {
            this.Dispose(false);
        }

        #endregion

        #region Eventos

        public event EventHandler<EventoListarTiendasCompletadoArgs> ListarTiendasCompletado;

        #endregion

        #region Funciones

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;
            
            if (BorrarCodigoAdministrado)
            {
                if (this.cliente != null)
                {
                    this.cliente.Dispose();
                    this.cliente = null;
                }
            }
        }

        protected void ManejarListarTiendasCompletado(object remitente, EventoListarTiendasCompletadoArgs args)
        {
            this.DispararListarTiendasCompletado(args);
        }

        protected virtual void DispararListarTiendasCompletado(EventoListarTiendasCompletadoArgs e)
        {
            if (this.ListarTiendasCompletado != null)
            {
                this.ListarTiendasCompletado(this, e);
            }
        }

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get 
            { 
                return this.estado; 
            }

            private set
            {
                if (value != this.estado)
                {
                    ConnectionState anterior = this.estado;
                    this.estado = value;
                    DispararCambioDeEstado(new StateChangeEventArgs(anterior, this.estado));
                }
            }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion

        #region Funciones

        #region Métodos sincrónicos

        public void Conectar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                this.Desconectar();

                this.cliente.UriWsdlServicio = this.DatosDeConexion.Anfitrion;
                this.cliente.Armar(usuario, contrasena);
                
                // Esto hay que borrarlo
                this.Estado = ConnectionState.Open;
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
                if (this.cliente != null)
                {
                    this.cliente.Desarmar();
                }
                
                // Esto hay que borrarlo
                this.Estado = ConnectionState.Closed;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar la conexión con el servidor de Netzuela", ex);
            }
        }

        public string[] ListarTiendas()
        {
            string[] resultado = new string[] { };

            try
            {
                resultado = this.cliente.ListarTiendas();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }

            return resultado;
        }

        public string[] ListarBasesDeDatos()
        {
            string[] resultado = new string[] {};

            try
            {
                resultado = this.cliente.ListarBasesDeDatos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
            
            return resultado;
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            string[] resultado = new string[] { };

            try
            {
                resultado = this.cliente.ListarTablas(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return resultado;
        }

        public DataTable LeerTabla(int tiendaId, string baseDeDatos, string tabla)
        {            
            DataTable resultado = new DataTable();

            try
            {
                resultado = this.cliente.LeerTabla(tiendaId, baseDeDatos, tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }

            return resultado;
        }

        public bool EscribirTabla(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            bool resultado = false;

            try
            {
                resultado = this.cliente.EscribirTabla(tiendaId, baseDeDatos, nombreTabla, tabla);
            }
            catch (Exception ex)
            {
                string error = "Error al escribir la tabla " + nombreTabla + " en la base de datos " + baseDeDatos;
                throw new Exception(error, ex);
            }

            return resultado;
        }

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarTiendasAsinc()
        {
            try
            {
                this.cliente.ListarTiendasAsinc();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
        }

        public void ListarBasesDeDatosAsinc()
        {
            try
            {
                this.cliente.ListarBasesDeDatosAsinc();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }
        }

        public void ListarTablasAsinc(string baseDeDatos)
        {
            try
            {
                this.cliente.ListarTablasAsinc(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas", ex);
            }
        }

        public void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla)
        {
            try
            {
                this.cliente.LeerTablaAsinc(tiendaId, baseDeDatos, tabla);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla", ex);
            }
        }

        public void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            try
            {
                this.cliente.EscribirTablaAsinc(tiendaId, baseDeDatos, nombreTabla, tabla);
            }
            catch (Exception ex)
            {
                string Error = "Error al escribir la tabla " + nombreTabla + " en la base de datos " + baseDeDatos;
                throw new Exception(Error, ex);
            }
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public void ConsultarAsinc(string baseDeDatos, string sql)
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

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #endregion
    }
}
