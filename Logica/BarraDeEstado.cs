using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;            // INotifyPropertyChanged
using System.Data;                      // ConnectionState

namespace Zuliaworks.Netzuela.Valeria.Logica
{
    /// <summary>
    ///  La función de esta clase es mantener un registro actualizado del estado de la aplicación.
    ///  Implementa la interfaz <see cref="INotifyPropertyChanged"/> para hacer visibles algunas 
    ///  de sus propiedades a la capa de presentación.
    /// </summary>
    public class BarraDeEstado : INotifyPropertyChanged
    {
        #region Variables

        private ConnectionState _EstadoConexion = ConnectionState.Closed;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea una barra de estado vacía.
        /// </summary>
        public BarraDeEstado() { }

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica el estado de la conexión en el formato establecido en <see cref="ConnectionState"/>
        /// </summary>
        public ConnectionState EstadoConexion
        {
            get { return _EstadoConexion; }
            set
            {
                if (value != _EstadoConexion)
                {
                    _EstadoConexion = value;
                    RegistrarCambioEnPropiedad("EstadoConexion");
                }
            }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        // ...

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
