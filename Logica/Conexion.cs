using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;               // ObservableCollection
using System.ComponentModel;                        // INotifyPropertyChanged
using System.Data;                                  // ConnectionState
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;            // SQLServer, MySQL, Oracle y Netzuela

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Esta clase permite establecer una conexión con cualquier fuente de datos compatible 
    /// de forma transparente para el programador.
    /// </summary>
    public class Conexion
    {
        #region Constructores

        /// <summary>
        /// Crea una conexión vacía.
        /// </summary>
        public Conexion()
        {
            this.Parametros = new ParametrosDeConexion();
            this.BD = new ServidorPredeterminado(this.Parametros);
        }

        /// <summary>
        /// Crea una conexión con el servidor especificado.
        /// </summary>
        /// <param name="Parametros">Datos de configuración de la conexión con el servidor.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="Parametros"/> es una referencia 
        /// nula.</exception>
        public Conexion(ParametrosDeConexion Parametros)
        {
            if (Parametros == null)
                throw new ArgumentNullException("Parametros");

            this.Parametros = Parametros;
            ResolverDatosDeConexion();
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

        #endregion

        #region Funciones

        public void ResolverDatosDeConexion()
        {
            switch (Parametros.Servidor)
            {
                case Constantes.SGBDR.SQL_SERVER:
                    BD = new SQLServer(Parametros);
                    break;
                case Constantes.SGBDR.ORACLE:
                    BD = new Oracle(Parametros);
                    break;
                case Constantes.SGBDR.MYSQL:
                    BD = new MySQL(Parametros);
                    break;
                case Constantes.SGBDR.NETZUELA:
                    BD = new Datos.Netzuela(Parametros);
                    break;
                default:
                    BD = new ServidorPredeterminado(Parametros);
                    break;
            }
        }

        /// <summary>
        /// Abre la conexión con el servidor especificado en <see cref="Parametros"/>.
        /// </summary>
        public void Conectar(SecureString Usuario, SecureString Contrasena)
        {
            try
            {
                if (BD == null)
                {
                    ResolverDatosDeConexion();
                }
                BD.Conectar(Usuario, Contrasena);
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
                if (BD != null)
                    BD.Desconectar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desconectarse de la base de datos", ex);
            }
        }

        public void CrearUsuario(SecureString Usuario, SecureString Contrasena, string[] ColumnasAutorizadas, int Privilegios)
        {
            try
            {
                BD.CrearUsuario(Usuario, Contrasena, ColumnasAutorizadas, Constantes.Privilegios.SELECCIONAR);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando el usuario en la base de datos", ex);
            }
        }

        #endregion
    }
}
