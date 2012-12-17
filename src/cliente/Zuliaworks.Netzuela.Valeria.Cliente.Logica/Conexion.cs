namespace Zuliaworks.Netzuela.Valeria.Cliente.Logica
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;               // ObservableCollection
    using System.ComponentModel;                        // INotifyPropertyChanged
    using System.Data;                                  // ConnectionState
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
    using Zuliaworks.Netzuela.Valeria.Datos;            // SQLServer, MySQL, Oracle y Netzuela
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;

    /// <summary>
    /// Esta clase permite establecer una conexión con cualquier fuente de datos compatible 
    /// de forma transparente para el programador.
    /// </summary>
    public class Conexion : Desechable
    {
        #region Constructores

        /// <summary>
        /// Crea una conexión vacía.
        /// </summary>
        public Conexion()
        {
            this.Parametros = new ParametrosDeConexion()
            {
                Anfitrion = string.Empty,
                Servidor = string.Empty,
                Instancia = string.Empty,
                ArgumentoDeConexion = string.Empty,
                MetodoDeConexion = string.Empty
            };

            this.BD = new ServidorPredeterminado(new ParametrosDeConexion()
            {
                Anfitrion = string.Empty,
                Servidor = string.Empty,
                Instancia = string.Empty,
                ArgumentoDeConexion = string.Empty,
                MetodoDeConexion = string.Empty
            });
        }

        /// <summary>
        /// Crea una conexión con el servidor especificado.
        /// </summary>
        /// <param name="parametros">Datos de configuración de la conexión con el servidor.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="parametros"/> es una referencia 
        /// nula.</exception>
        public Conexion(ParametrosDeConexion parametros)
        {
            if (parametros == null)
            {
                throw new ArgumentNullException("parametros");
            }

            this.Parametros = parametros;
            this.ResolverDatosDeConexion();
        }

        ~Conexion()
        {
            this.Dispose(false);
        }

        #endregion

        #region Eventos

        public event StateChangeEventHandler CambioDeEstado
        {
            add { this.BD.CambioDeEstado += value; }
            remove { this.BD.CambioDeEstado -= value; }
        }

        public event EventHandler<EventoListarTiendasCompletadoArgs> ListarTiendasCompletado
        {
            add 
            { 
                var tipo = this.BD.GetType();
                if (tipo.GetEvent("ListarTiendasCompletado") != null)
                {
                    ((INetzuela)this.BD).ListarTiendasCompletado += value;
                }
                else
                {
                    throw new MissingMemberException();
                }
            }
            remove 
            {
                var tipo = this.BD.GetType();
                if (tipo.GetEvent("ListarTiendasCompletado") != null)
                {
                    ((INetzuela)this.BD).ListarTiendasCompletado -= value;
                }
                else
                {
                    throw new MissingMemberException();
                }
            }
        }

        public event EventHandler<EventoListarBDsCompletadoArgs> ListarBasesDeDatosCompletado
        {
            add { this.BD.ListarBasesDeDatosCompletado += value; }
            remove { this.BD.ListarBasesDeDatosCompletado -= value; }
        }

        public event EventHandler<EventoListarTablasCompletadoArgs> ListarTablasCompletado
        {
            add { this.BD.ListarTablasCompletado += value; }
            remove { this.BD.ListarTablasCompletado -= value; }
        }

        public event EventHandler<EventoLeerTablaCompletadoArgs> LeerTablaCompletado
        {
            add { this.BD.LeerTablaCompletado += value; }
            remove { this.BD.LeerTablaCompletado -= value; }
        }

        public event EventHandler<EventoEscribirTablaCompletadoArgs> EscribirTablaCompletado
        {
            add { this.BD.EscribirTablaCompletado += value; }
            remove { this.BD.EscribirTablaCompletado -= value; }
        }

        public event EventHandler<EventoCrearUsuarioCompletadoArgs> CrearUsuarioCompletado
        {
            add { this.BD.CrearUsuarioCompletado += value; }
            remove { this.BD.CrearUsuarioCompletado -= value; }
        }

        public event EventHandler<EventoConsultarCompletadoArgs> ConsultarCompletado
        {
            add { this.BD.ConsultarCompletado += value; }
            remove { this.BD.ConsultarCompletado -= value; }
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Parámetros de la conexión con el servidor actual.
        /// </summary>
        public ParametrosDeConexion Parametros { get; set; }

        /// <summary>
        /// Proveedor de datos creado a partir de las especificaciones de conexión.
        /// </summary>
        public IBaseDeDatos BD { get; private set; }

        public ConnectionState Estado 
        { 
            get { return this.BD.Estado; }
        }

        #endregion

        #region Funciones

        public void ResolverDatosDeConexion()
        {
            if (this.BD != null && this.BD.DatosDeConexion.Servidor == this.Parametros.Servidor)
            {
                this.BD.DatosDeConexion = this.Parametros;
            }
            else
            {
                if (this.BD != null)
                {
                    this.BD.Dispose();
                    this.BD = null;
                }

                switch (this.Parametros.Servidor)
                {
                    case RDBMS.SqlServer:
                        this.BD = new SQLServer(this.Parametros);
                        break;
                    case RDBMS.DbIsam:
                        this.BD = new DBISAM(this.Parametros);
                        break;
                    case RDBMS.MySQL:
                        this.BD = new MySQL(this.Parametros);
                        break;
                    case RDBMS.Netzuela:
                        this.BD = new Datos.Netzuela(this.Parametros);
                        break;
                    default:
                        this.BD = new ServidorPredeterminado(this.Parametros);
                        break;
                }
            }
        }

        #region Métodos sincrónicos
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="contrasena"></param>
        public void Conectar(SecureString usuario, SecureString contrasena)
        {
            try
            {
                this.BD.Conectar(usuario, contrasena);
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando con la base de datos", ex);
            }
        }

        /// <summary>
        /// Cierra la conexión con el servidor.
        /// </summary>
        public void Desconectar()
        {
            try
            {
                this.BD.Desconectar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desconectarse de la base de datos", ex);
            }
        }

        public string[] ListarTiendas()
        {
            string[] resultado = null;

            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("ListarTiendas") != null)
                {
                    resultado = ((INetzuela)this.BD).ListarTiendas();
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tiendas", ex);
            }

            return resultado;
        }

        public string[] ListarBasesDeDatos()
        {
            string[] resultado = null;

            try
            {
                resultado = this.BD.ListarBasesDeDatos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las bases de datos", ex);
            }

            return resultado;
        }

        public string[] ListarTablas(string baseDeDatos)
        {
            string[] resultado = null;

            try
            {
                resultado = this.BD.ListarTablas(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas de la base de datos \"" + baseDeDatos + "\"", ex);
            }

            return resultado;
        }

        public DataTable LeerTabla(string baseDeDatos, string tabla)
        {
            DataTable resultado = null;

            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("LeerTabla") != null)
                {
                    resultado = ((IBaseDeDatosLocal)this.BD).LeerTabla(baseDeDatos, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla \"" + tabla + "\" de la base de datos \"" + baseDeDatos + "\"", ex);
            }

            return resultado;
        }

        public DataTable LeerTabla(int tiendaId, string baseDeDatos, string tabla)
        {
            DataTable resultado = null;

            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("LeerTabla") != null)
                {
                    resultado = ((INetzuela)this.BD).LeerTabla(tiendaId, baseDeDatos, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla \"" + tabla + "\" de la base de datos \"" + baseDeDatos + "\"", ex);
            }

            return resultado;
        }

        public bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            bool resultado = false;

            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("EscribirTabla") != null)
                {
                    resultado = ((IBaseDeDatosLocal)this.BD).EscribirTabla(baseDeDatos, nombreTabla, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al escribir la tabla \"" + tabla + "\" en la base de datos \"" + baseDeDatos + "\"", ex);
            }

            return resultado;
        }

        public bool EscribirTabla(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            bool resultado = false;

            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("EscribirTabla") != null)
                {
                    resultado = ((INetzuela)this.BD).EscribirTabla(tiendaId, baseDeDatos, nombreTabla, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al escribir la tabla \"" + tabla + "\" en la base de datos \"" + baseDeDatos + "\"", ex);
            }

            return resultado;
        }

        public bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnasAutorizadas, int privilegios)
        {
            bool resultado = false;

            try
            {
                resultado = this.BD.CrearUsuario(usuario, contrasena, columnasAutorizadas, Privilegios.Seleccionar);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando el usuario en la base de datos", ex);
            }

            return resultado;
        }

        public DataTable Consultar(string baseDeDatos, string sql)
        {
            DataTable resultado = null;

            try
            {
                resultado = this.BD.Consultar(baseDeDatos, sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al consultar la base de datos", ex);
            }

            return resultado;
        }

        #endregion

        #region Métodos asincrónicos

        public void ListarTiendasAsinc()
        {
            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("ListarTiendasAsinc") != null)
                {
                    ((INetzuela)this.BD).ListarTiendasAsinc();
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tiendas", ex);
            }
        }

        public void ListarBasesDeDatosAsinc()
        {
            try
            {
                this.BD.ListarBasesDeDatosAsinc();
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
                this.BD.ListarTablasAsinc(baseDeDatos);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las tablas de la base de datos \"" + baseDeDatos + "\"", ex);
            }
        }

        public void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("LeerTablaAsinc") != null)
                {
                    ((IBaseDeDatosLocal)this.BD).LeerTablaAsinc(baseDeDatos, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla \"" + tabla + "\" de la base de datos \"" + baseDeDatos + "\"", ex);
            }
        }

        public void LeerTablaAsinc(int tiendaId, string baseDeDatos, string tabla)
        {
            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("LeerTablaAsinc") != null)
                {
                    ((INetzuela)this.BD).LeerTablaAsinc(tiendaId, baseDeDatos, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la tabla \"" + tabla + "\" de la base de datos \"" + baseDeDatos + "\"", ex);
            }
        }

        public void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("EscribirTablaAsinc") != null)
                {
                    ((IBaseDeDatosLocal)this.BD).EscribirTablaAsinc(baseDeDatos, nombreTabla, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al escribir la tabla \"" + tabla + "\" en la base de datos \"" + baseDeDatos + "\"", ex);
            }
        }

        public void EscribirTablaAsinc(int tiendaId, string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            try
            {
                var tipo = this.BD.GetType();
                if (tipo.GetMethod("EscribirTablaAsinc") != null)
                {
                    ((INetzuela)this.BD).EscribirTablaAsinc(tiendaId, baseDeDatos, nombreTabla, tabla);
                }
                else
                {
                    throw new MissingMethodException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al escribir la tabla \"" + tabla + "\" en la base de datos \"" + baseDeDatos + "\"", ex);
            }
        }

        public void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            try
            {
                this.BD.CrearUsuarioAsinc(usuario, contrasena, columnas, privilegios);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando el usuario en la base de datos", ex);
            }
        }

        public void ConsultarAsinc(string baseDeDatos, string sql)
        {
            try
            {
                this.BD.ConsultarAsinc(baseDeDatos, sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al consultar la base de datos", ex);
            }
        }

        #endregion
        
        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            this.Parametros = null;

            if (borrarCodigoAdministrado)
            {
                this.BD.Dispose();
            }
        }
        
        #endregion
    }
}