using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;   // ObservableCollection
using System.ComponentModel;            // INotifyPropertyChanged
using System.Data;                      // ConnectionState
using System.Security;                  // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;                  // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;                    // SQLServer, MySQL y Oracle

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    /// Esta clase permite establecer una conexión con cualquier fuente de datos compatible 
    /// de forma transparente para el programador. Implementa la interfaz 
    /// <see cref="INotifyPropertyChanged"/> para hacer visibles algunas de sus propiedades 
    /// a la capa de presentación.
    /// </summary>
    public class Conexion : INotifyPropertyChanged
    {
        #region Variables

        /// <summary>
        /// Proveedor de datos creado a partir de las especificaciones de conexión.
        /// </summary>
        public IBaseDeDatos BD;
        private DatosDeConexion _Datos;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea una conexión vacía.
        /// </summary>
        public Conexion()
        {
            this.Datos = new DatosDeConexion();
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
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Parámetros de la conexión con el servidor actual.
        /// </summary>
        public DatosDeConexion Datos
        {
            get { return _Datos; }
            set
            {
                if (value != _Datos)
                {
                    _Datos = value;
                    RegistrarCambioEnPropiedad("Datos");
                }
            }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        /// <summary>
        /// Abre la conexión con el servidor especificado en <see cref="Datos"/>.
        /// </summary>
        public void Conectar(SecureString Usuario, SecureString Contrasena)
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

            try
            {
                BD.Conectar(Usuario, Contrasena);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error conectando con la base de datos: " + ex.Message + ex.InnerException);
                throw new Exception("Error conectando con la base de datos", ex);
            }
        }

        /// <summary>
        /// Cierra la conexión con el servidor.
        /// </summary>
        public void Desconectar()
        {
            if (BD != null)
                BD.Desconectar();
        }

        /// <summary>
        /// Obtiene los detalles de conexión de todos los servidores detectados en el equipo local.
        /// </summary>
        public static List<ServidorLocal> DetectarServidoresLocales()
        {
            return ServidorLocal.DetectarTodos();
        }

        #endregion

        #region Implementaciones de interfaces

        /// <summary>
        /// Evento que se activa cuando una propiedad de esta clase ha sido modificada.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Esta función se llama de forma interna cuando se cambia una propiedad de esta clase
        /// </summary>
        /// <param name="info">Nombre de la propiedad modificada.</param>
        protected virtual void RegistrarCambioEnPropiedad(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
