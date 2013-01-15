namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                              // ConnectionState, DataTable
    using System.Linq;
    using System.Security;                          // SecureString
    using System.Text;
    using System.Data.Common;                       // DbConnection

    using MySql.Data.MySqlClient;                   // MySqlConnection
    using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

    /// <summary>
    /// Implementa las funciones genericas de acceso a las bases de datos
    /// </summary>
    public abstract class ConectorGenerico<T, U, V> : EventosComunes, IBaseDeDatosLocal
        where T : DbConnection, new()
        where U : DbCommand, new()
        where V : DbDataAdapter
    {
        #region Variables y Constantes

        protected T conexion;

        #endregion
        
        #region Constructores

        public ConectorGenerico(ParametrosDeConexion servidorBd)
        {
            this.DatosDeConexion = servidorBd;
            this.conexion = new T();
            this.conexion.StateChange -= this.ManejarCambioDeEstado;
            this.conexion.StateChange += this.ManejarCambioDeEstado;
        }

        #endregion

        #region Funciones

        protected virtual void Dispose(bool borrarCodigoAdministrado)
        {
            this.DatosDeConexion = null;

            if (borrarCodigoAdministrado)
            {
                if (this.conexion != null)
                {
                    this.conexion.Dispose();
                    this.conexion = null;
                }
            }
        }

        protected virtual void CambiarBaseDeDatos(string baseDeDatos)
        {
            if (this.conexion.Database != baseDeDatos)
            {
                this.conexion.ChangeDatabase(baseDeDatos);
            }
        }

        protected virtual void EjecutarOrden(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            using (U orden = new U())
            {
                orden.CommandText = sql;
                orden.Connection = this.conexion;
                orden.ExecuteNonQuery();
            }
        }

        protected virtual string[] LectorSimple(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            List<string> resultado = new List<string>();
            using (U orden = new U())
            {
                orden.CommandText = sql;
                orden.Connection = this.conexion;
                using (var lector = orden.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        resultado.Add(lector.GetString(0));
                    }
                }
            }

            return resultado.ToArray();
        }

        protected virtual DataTable LectorAvanzado(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            DataSet resultado = new DataSet();

            // Para pasar argumentos al constructor de un tipo dinamico:
            // http://www.dalun.com/blogs/05.27.2007.htm

            var ctorV = typeof(V).GetConstructor(new System.Type[] { typeof(string), typeof(T) });
            using (V adaptador = (V) ctorV.Invoke(new object[] { sql, this.conexion }))
            {
                adaptador.FillSchema(resultado, SchemaType.Source);
                adaptador.Fill(resultado);
            }

            return resultado.Tables[0];
        }

        protected abstract string DescribirTabla(string tabla);
        protected abstract SecureString CrearRutaDeAcceso(ParametrosDeConexion seleccion, SecureString usuario, SecureString contrasena);

        #endregion

        #region Implementaciones de interfaces

        #region Propiedades

        public ConnectionState Estado
        {
            get { return this.conexion.State; }
        }

        public ParametrosDeConexion DatosDeConexion { get; set; }

        #endregion

        #region Funciones

        #region Métodos sincrónicos

        public abstract void Conectar(SecureString usuario, SecureString contrasena);
        public abstract void Desconectar();
        public abstract string[] ListarBasesDeDatos();
        public abstract string[] ListarTablas(string baseDeDatos);
        public abstract DataTable LeerTabla(string baseDeDatos, string tabla);
        public abstract bool EscribirTabla(string baseDeDatos, string nombreTabla, DataTable tabla);
        public abstract bool CrearUsuario(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios);
        public abstract DataTable Consultar(string baseDeDatos, string sql);

        #endregion

        #region Métodos asincrónicos

        public virtual void ListarBasesDeDatosAsinc()
        {
            throw new NotImplementedException();
        }

        public virtual void ListarTablasAsinc(string baseDeDatos)
        {
            throw new NotImplementedException();
        }

        public virtual void LeerTablaAsinc(string baseDeDatos, string tabla)
        {
            throw new NotImplementedException();
        }

        public virtual void EscribirTablaAsinc(string baseDeDatos, string nombreTabla, DataTable tabla)
        {
            throw new NotImplementedException();
        }

        public virtual void CrearUsuarioAsinc(SecureString usuario, SecureString contrasena, string[] columnas, int privilegios)
        {
            throw new NotImplementedException();
        }

        public virtual void ConsultarAsinc(string baseDeDatos, string sql)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public virtual void Dispose()
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
