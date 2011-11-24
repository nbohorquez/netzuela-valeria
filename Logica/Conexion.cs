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
            this.Datos = new DatosDeConexion();
            this.BD = new ServidorPredeterminado(this.Datos);
        }

        /// <summary>
        /// Crea una conexión con el servidor especificado.
        /// </summary>
        /// <param name="Datos">Datos de configuración de la conexión con el servidor.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="Datos"/> es una referencia 
        /// nula.</exception>
        public Conexion(DatosDeConexion Datos)
        {
            if (Datos == null)
                throw new ArgumentNullException("Datos");

            this.Datos = Datos;
            ResolverDatosDeConexion();
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Parámetros de la conexión con el servidor actual.
        /// </summary>
        public DatosDeConexion Datos { get; set; }

        /// <summary>
        /// Proveedor de datos creado a partir de las especificaciones de conexión.
        /// </summary>
        public IBaseDeDatos BD { get; private set; }

        #endregion

        #region Funciones

        public void ResolverDatosDeConexion()
        {
            switch (Datos.Servidor)
            {
                case Constantes.SGBDR.SQL_SERVER:
                    BD = new SQLServer(Datos);
                    break;
                case Constantes.SGBDR.ORACLE:
                    BD = new Oracle(Datos);
                    break;
                case Constantes.SGBDR.MYSQL:
                    BD = new MySQL(Datos);
                    break;
                case Constantes.SGBDR.NETZUELA:
                    BD = new Datos.Netzuela(Datos);
                    break;
                default:
                    BD = new ServidorPredeterminado(Datos);
                    break;
            }
        }

        /// <summary>
        /// Abre la conexión con el servidor especificado en <see cref="Datos"/>.
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
