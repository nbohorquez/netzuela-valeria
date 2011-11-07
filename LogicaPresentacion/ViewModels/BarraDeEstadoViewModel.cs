using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;               // ObservableObject
using System.Data;                      // ConnectionState

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    ///  La función de esta clase es mantener un registro actualizado del estado de la aplicación.
    /// </summary>
    public class BarraDeEstadoViewModel : ObservableObject
    {
        #region Variables

        private ConnectionState _EstadoConexion;

        #endregion

        #region Constructores

        /// <summary>
        /// Crea una barra de estado vacía.
        /// </summary>
        public BarraDeEstadoViewModel() 
        {
            EstadoConexion = ConnectionState.Closed;
        }

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
                    RaisePropertyChanged("EstadoConexion");
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

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
